//chấm điểm quyền pháp
const baseUrlQuyen = (window.appBase || '/');
const normalizedBaseUrlQuyen = baseUrlQuyen.endsWith('/') ? baseUrlQuyen : `${baseUrlQuyen}/`;


document.addEventListener('DOMContentLoaded', function () {
    // Quản lý trạng thái cảnh cáo
    const cautionState = {
        blue: 0,
        red: 0
    };



    // Xử lý sự kiện click cho các nút chấm điểm
    document.addEventListener('click', function (event) {
        const btn = event.target.closest('.score-btn');
        if (!btn) return;

        // Lấy thông tin từ nút
        const target = btn.getAttribute('data-target');
        const value = btn.getAttribute('data-value');
        const scoreElement = document.getElementById(`${target}-score`);

        // Xử lý từng loại điểm
        if (value === 'caution') {
            // Quản lý cảnh cáo
            cautionState[target]++;

            // Hiển thị thông báo cảnh cáo
            const cautionMessage = `Cảnh cáo ${target === 'blue' ? 'Xanh' : 'Đỏ'} (Lần: ${cautionState[target]})`;

            // Sử dụng SweetAlert để thông báo
            Swal.fire({
                icon: 'warning',
                title: 'Cảnh Cáo',
                text: cautionMessage,
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000
            });

            // Xử lý logic khi số lần cảnh cáo vượt quá giới hạn
            if (cautionState[target] >= 3) {
                Swal.fire({
                    icon: 'error',
                    title: 'Truất Quyền',
                    text: `Vận động viên ${target === 'blue' ? 'Xanh' : 'Đỏ'} bị truất quyền do quá 3 lần cảnh cáo`,
                    confirmButtonText: 'Xác Nhận'
                });

                // Đặt điểm về 0 khi bị truất quyền
                document.getElementById(`${target}-score`).textContent = '0';
            }
            return;
        }

        // Xử lý điểm số
        const currentScore = parseInt(scoreElement.textContent);
        const newScore = currentScore + parseInt(value);

        // Đảm bảo điểm không âm
        scoreElement.textContent = Math.max(0, newScore);

        // Hiệu ứng nhấp nháy khi thay đổi điểm
        scoreElement.classList.add('score-changed');
        setTimeout(() => {
            scoreElement.classList.remove('score-changed');
        }, 300);
    });

    // Xử lý submit kết quả
    const submitButton = document.getElementById('submit-combat');
    if (submitButton) {
        submitButton.addEventListener('click', function () {
            // Lấy kết quả đã chọn
            const selectedResult = document.querySelector('input[name="combat-result"]:checked').value;

            // Lấy điểm số
            const blueScore = document.getElementById('blue-score').textContent;
            const redScore = document.getElementById('red-score').textContent;

            // Xác nhận trước khi submit
            Swal.fire({
                title: 'Xác Nhận Kết Quả',
                html: `
                        <p>Điểm Xanh: ${blueScore}</p>
                        <p>Điểm Đỏ: ${redScore}</p>
                        <p>Kết Quả: ${selectedResult}</p>
                        <p>Cảnh Cáo Xanh: ${cautionState.blue}</p>
                        <p>Cảnh Cáo Đỏ: ${cautionState.red}</p>
                    `,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Xác Nhận',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Gọi hàm submit kết quả
                    submitMatchResult({
                        blueScore: blueScore,
                        redScore: redScore,
                        result: selectedResult,
                        blueCautions: cautionState.blue,
                        redCautions: cautionState.red
                    });
                }
            });
        });
    }
});

//hamxuly
function submitMatchResult(matchResult) {
    // Lấy thông tin trận đấu từ localStorage
    const currentMatch = JSON.parse(localStorage.getItem('currentMatch')) || {};

    // Xác định các ID vận động viên
    const blueAthleteId = blueParticipantId;
    const redAthleteId = redParticipantId;
    const matchId = currentMatchId;

    // Validate dữ liệu
    if (!matchId || !blueAthleteId || !redAthleteId) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Thiếu thông tin trận đấu'
        });
        return;
    }

    // Xác nhận trước khi submit
    Swal.fire({
        title: 'Xác nhận kết quả',
        html: `
                <p>Điểm Xanh: ${matchResult.blueScore}</p>
                <p>Điểm Đỏ: ${matchResult.redScore}</p>
                <p>Kết quả: ${matchResult.result}</p>
            `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            // Chuẩn bị payload để gửi lên server
            const payload = {
                matchId: matchId,
                blueParticipantId: blueAthleteId,
                redParticipantId: redAthleteId,
                blueScore: matchResult.blueScore,
                redScore: matchResult.redScore,
                blueCautions: matchResult.blueCautions || 0,
                redCautions: matchResult.redCautions || 0,
                result: matchResult.result
            };

            // Gọi API submit kết quả
            fetch(`${normalizedBaseUrlQuyen}ChamDiem/SubmitMatchResult`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(payload)
            })
                .then(response => {
                    if (!response.ok) {
                        // Xử lý lỗi từ server
                        return response.json().then(errorData => {
                            throw new Error(errorData.message || 'Lỗi khi submit kết quả');
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    // Hiển thị thông báo thành công
                    Swal.fire({
                        icon: 'success',
                        title: 'Cập Nhật Thành Công',
                        html: `
                            <p>Đã lưu kết quả trận đấu</p>
                            <p>Trạng Thái: ${data.matchStatus}</p>
                        `,
                        confirmButtonText: 'OK'
                    }).then(() => {
                        // Chuyển hướng sau khi lưu thành công
                        window.location.href = `${normalizedBaseUrlQuyen}Home/Index`;
                    });
                })
                .catch(error => {
                    // Xử lý lỗi
                    console.error('Lỗi khi submit kết quả:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: error.message || 'Không thể lưu kết quả. Vui lòng thử lại.',
                        confirmButtonText: 'Đóng'
                    });
                });
        }
    });
}

// Hàm hỗ trợ để xác định kết quả (nếu cần)
function determineMatchResult(blueScore, redScore) {
    if (blueScore > redScore) return 'BlueWin';
    if (redScore > blueScore) return 'RedWin';
    return 'Draw';
}

// Hàm để chuẩn bị dữ liệu từ giao diện
function prepareMatchResultFromUI() {
    // Lấy điểm số từ giao diện
    const blueScore = document.getElementById('blue-score').textContent;
    const redScore = document.getElementById('red-score').textContent;

    // Lấy số lần cảnh cáo (nếu có)
    const blueCautions = document.getElementById('blue-cautions')?.textContent || 0;
    const redCautions = document.getElementById('red-cautions')?.textContent || 0;

    // Xác định kết quả
    const result = determineMatchResult(
        parseFloat(blueScore),
        parseFloat(redScore)
    );

    // Trả về đối tượng kết quả
    return {
        blueScore: blueScore,
        redScore: redScore,
        blueCautions: parseInt(blueCautions),
        redCautions: parseInt(redCautions),
        result: result
    };
}