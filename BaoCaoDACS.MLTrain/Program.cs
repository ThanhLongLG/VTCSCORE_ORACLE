using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BaoCaoDACS;
using BaoCaoDACS.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

class Program
{
    static float GetRatingBeforeMatch(AppDbContext context, string? userId, int? tournamentId, DateTime matchDate)
    {
        // default Elo/rating nếu chưa có ranking
        const float DEFAULT_RATING = 1000f;

        if (string.IsNullOrWhiteSpace(userId) || tournamentId is null)
            return DEFAULT_RATING;

        // Lấy rating gần nhất TRƯỚC (hoặc bằng) thời điểm trận diễn ra để tránh leak
        var rating = context.TournamentRankings
            .Where(r => r.UserId == userId
                     && r.TournamentId == tournamentId.Value
                     && r.UpdatedAt <= matchDate)
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => (float?)r.Rating)
            .FirstOrDefault();

        return rating ?? DEFAULT_RATING;
    }

    static void Main(string[] args)
    {
        // 1) Load cấu hình
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("QLTAPVO");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseOracle(connectionString)
            .Options;

        using var context = new AppDbContext(options);
        Console.WriteLine("Kết nối DB thành công!");

        // 2) Lấy dữ liệu trận đấu đã hoàn thành
        var finishedMatches = context.match
            .Include(m => m.Socre)
                .ThenInclude(s => s.participant)
            .Where(m => m.Socre.Count == 2 && m.Socre.All(s => s.Kq != null))
            .ToList();

        Console.WriteLine($"Có {finishedMatches.Count} trận đã hoàn thành.");

        var trainingData = new List<MatchTrainingSample>();

        foreach (var match in finishedMatches)
        {
            // bảo vệ null
            if (match.TournamentID == null) continue;

            var scores = match.Socre.ToList();

            var winnerScore = scores.FirstOrDefault(s => s.Kq == 1);
            var loserScore = scores.FirstOrDefault(s => s.Kq == 0);
            if (winnerScore == null || loserScore == null) continue;

            var pWinner = winnerScore.participant;
            var pLoser = loserScore.participant;
            if (pWinner == null || pLoser == null) continue;

            // lấy rating theo từng giải (TournamentId) + userId, trước thời điểm trận
            var winnerRating = GetRatingBeforeMatch(context, pWinner.UserId, match.TournamentID, match.Date);
            var loserRating = GetRatingBeforeMatch(context, pLoser.UserId, match.TournamentID, match.Date);

            // Sample 1: A = winner, B = loser
            trainingData.Add(new MatchTrainingSample
            {
                FighterA_Weight = pWinner.CanNang ?? 0,
                FighterA_Height = pWinner.ChieuCao ?? 0,
                FighterA_Age = pWinner.tuoi ?? 0,

                FighterB_Weight = pLoser.CanNang ?? 0,
                FighterB_Height = pLoser.ChieuCao ?? 0,
                FighterB_Age = pLoser.tuoi ?? 0,

                FighterA_Rating = winnerRating,
                FighterB_Rating = loserRating,
                RatingDiff = winnerRating - loserRating,

                LoaiHinhThiDauId = match.LoaiHinhThiDauId,
                HangCan = match.Hangcan ?? "",
                VongDau = match.Vongdau ?? "",

                DiffWeight = (pWinner.CanNang ?? 0) - (pLoser.CanNang ?? 0),
                DiffHeight = (pWinner.ChieuCao ?? 0) - (pLoser.ChieuCao ?? 0),
                DiffAge = (pWinner.tuoi ?? 0) - (pLoser.tuoi ?? 0),

                AWins = true
            });

            // Sample 2: A = loser, B = winner
            trainingData.Add(new MatchTrainingSample
            {
                FighterA_Weight = pLoser.CanNang ?? 0,
                FighterA_Height = pLoser.ChieuCao ?? 0,
                FighterA_Age = pLoser.tuoi ?? 0,

                FighterB_Weight = pWinner.CanNang ?? 0,
                FighterB_Height = pWinner.ChieuCao ?? 0,
                FighterB_Age = pWinner.tuoi ?? 0,

                FighterA_Rating = loserRating,
                FighterB_Rating = winnerRating,
                RatingDiff = loserRating - winnerRating,

                LoaiHinhThiDauId = match.LoaiHinhThiDauId,
                HangCan = match.Hangcan ?? "",
                VongDau = match.Vongdau ?? "",

                DiffWeight = (pLoser.CanNang ?? 0) - (pWinner.CanNang ?? 0),
                DiffHeight = (pLoser.ChieuCao ?? 0) - (pWinner.ChieuCao ?? 0),
                DiffAge = (pLoser.tuoi ?? 0) - (pWinner.tuoi ?? 0),

                AWins = false
            });
        }

        Console.WriteLine($"Training data: {trainingData.Count} dòng.");
        Console.WriteLine("Số mẫu AWins = true : " + trainingData.Count(x => x.AWins));
        Console.WriteLine("Số mẫu AWins = false: " + trainingData.Count(x => !x.AWins));

        if (trainingData.Count == 0)
        {
            Console.WriteLine("❌ Không có dữ liệu để train. Kiểm tra lại bảng Match/Socre/Kq.");
            return;
        }

        // 3) Train ML.NET
        var mlContext = new MLContext(seed: 1);

        var data = mlContext.Data.LoadFromEnumerable(trainingData);
        var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2, seed: 1);

        var pipeline =
            mlContext.Transforms.Categorical.OneHotEncoding(
                new[]
                {
                    new InputOutputColumnPair("HangCanEncoded", "HangCan"),
                    new InputOutputColumnPair("VongDauEncoded", "VongDau"),
                    new InputOutputColumnPair("LoaiHinhEncoded", "LoaiHinhThiDauId")
                })
            .Append(mlContext.Transforms.Conversion.ConvertType(
                new[]
                {
                    new InputOutputColumnPair("FighterA_Age_f", "FighterA_Age"),
                    new InputOutputColumnPair("FighterB_Age_f", "FighterB_Age"),
                },
                DataKind.Single))

            .Append(mlContext.Transforms.Concatenate("Features",
                "FighterA_Weight", "FighterA_Height", "FighterA_Age_f",
                "FighterB_Weight", "FighterB_Height", "FighterB_Age_f",
                "FighterA_Rating", "FighterB_Rating", "RatingDiff",
                "DiffWeight", "DiffHeight", "DiffAge",
                "LoaiHinhEncoded",
                "HangCanEncoded", "VongDauEncoded"
            ))

          // normalize numeric cho ổn định hơn
            .Append(mlContext.Transforms.NormalizeMeanVariance("Features"))


            .Append(mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(MatchTrainingSample.AWins),
                featureColumnName: "Features"));

        var model = pipeline.Fit(split.TrainSet);

        var predictions = model.Transform(split.TestSet);
        var metrics = mlContext.BinaryClassification.Evaluate(
            predictions,
            labelColumnName: nameof(MatchTrainingSample.AWins));

        Console.WriteLine($"🎯 Accuracy: {metrics.Accuracy:0.####}");
        Console.WriteLine($"🎯 AUC:      {metrics.AreaUnderRocCurve:0.####}");
        Console.WriteLine($"🎯 F1Score:  {metrics.F1Score:0.####}");

        // 4) Save model (CHỈ 1 lần)
        const string modelFile = "match_predictor.zip";
        mlContext.Model.Save(model, split.TrainSet.Schema, modelFile);
        Console.WriteLine($"✅ Đã lưu model vào {modelFile}");

        // 5) Copy vào web
        var currentDir = Directory.GetCurrentDirectory();
        var targetPath = Path.GetFullPath(Path.Combine(
            currentDir,
            @"..\..\..\..\BaoCaoDACS\wwwroot\Models\match_predictor.zip"
        ));

        var targetDir = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir!);

        File.Copy(modelFile, targetPath, overwrite: true);
        Console.WriteLine($"✅ Đã copy model vào web: {targetPath}");
    }
}
