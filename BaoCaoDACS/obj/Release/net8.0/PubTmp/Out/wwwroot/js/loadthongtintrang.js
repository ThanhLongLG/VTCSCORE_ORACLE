



// Hàm tạo thẻ section từ 1 trận đấu
function createMatchHtml(match) {
    const formattedDate = match?.date
        ? new Date(match.date).toLocaleString('vi-VN')
        : new Date().toLocaleString('vi-VN');
    return `
          <section class="match-info tab-match-info active" id="match-info-${match.matchId}">
              <div class="match-header">
                  <h2 class="match-title">Trận đấu: Võ thuật - Hạng cân ${match.hangcan ?? 'N/A'}</h2>
                  <span class="match-id">Mã trận: ${match.matchId}</span>
              </div>
              <div class="match-details">
                  <div class="match-detail">
                      <span class="detail-label">Thời gian</span>
                      <span class="detail-value">${(match.date)}</span>
                  </div>
                    <div class="match-detail">
                      <span class="detail-label">Thời gian hiện tại</span>
                      <span class="detail-value">${formattedDate}</span>
                  </div>
                  <div class="match-detail">
                      <span class="detail-label">Sân đấu</span>
                      <span class="detail-value">${match.sanDau}</span>
                  </div>
                  <div class="match-detail">
                      <span class="detail-label">Vòng đấu</span>
                      <span class="detail-value">${match.vongdau}</span>
                  </div>
                  <div class="match-detail">
                      <span class="detail-label">Trọng tài chính</span>
                      <span class="detail-value">${match.trongtai}</span>
                  </div>
              </div>
          </section>
          `;

}

// Gọi API khi trang tải xong
document.addEventListener("DOMContentLoaded", function () {
    // Lấy matchId từ URL
    const urlParams = new URLSearchParams(window.location.search);
    const matchId = urlParams.get('matchId');

    // Kiểm tra nếu không có matchId
    if (!matchId) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Không tìm thấy mã trận đấu',
            confirmButtonText: 'Quay lại'
        }).then(() => {
            const baseUrl = (window.appBase || '/');
            const normalizedBaseUrl = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
            window.location.href = `${normalizedBaseUrl}Home/Index`; // Điều hướng về trang chủ
        });
        return;
    }

    const baseUrl = (window.appBase || '/');
    const normalizedBaseUrl = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;

    // Gọi API với matchId cụ thể
    fetch(`${normalizedBaseUrl}ChamDiem/GetMatch?matchId=${encodeURIComponent(matchId)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Không thể tải thông tin trận đấu');
            }
            return response.json();
        })
        .then(match => {
            if (!match) {
                throw new Error('Không tìm thấy thông tin trận đấu');
            }

            const container = document.getElementById("match-container");

            // Xóa nội dung cũ (nếu có)
            container.innerHTML = '';

            // Render thẻ HTML cho trận đấu
            const html = createMatchHtml(match);
            container.innerHTML = html;

            // Có thể thêm các xử lý khác sau khi render
            console.log("Thông tin trận đấu đầy đủ:", match);
            TournamentId = match.tournament || match.Tournament;
            if (typeof currentMatchId !== 'undefined') {
                currentMatchId = match.matchId;
            }
            console.log("Thông tin:", TournamentId);
            setupMatchUI(match);
            const fetchParticipantsFn = window.fetchMatchParticipants;
            if (typeof fetchParticipantsFn !== 'function') {
                throw new Error('fetchMatchParticipants is not defined');
            }
            fetchParticipantsFn(match);
            if (typeof window.renderTournamentResults === 'function' && TournamentId) {
                window.renderTournamentResults(TournamentId);
            }

        })
        .catch(error => {
            console.error("Lỗi khi tải thông tin trận đấu:", error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: error.message,
                confirmButtonText: 'Đóng'
            });
        });
});
function setupMatchUI(match) {
    // Lấy các tab
    const tabs = document.querySelectorAll('.scoring-tabs .tab');
    const tabContents = document.querySelectorAll('.tab-content');

    // Mặc định vô hiệu hóa tất cả các tab
    tabs.forEach(tab => {
        tab.classList.remove('active');
        tab.classList.add('disabled');
        tab.style.pointerEvents = 'none'; // Vô hiệu hóa click
        tab.style.opacity = '0.5'; // Làm mờ tab
    });

    // Ẩn tất cả nội dung tab
    tabContents.forEach(content => {
        content.classList.remove('active');
        content.style.display = 'none';
    });

    // Log thông tin để kiểm tra

    console.log("Loại hình thi đấu:", match.loaiHinhThiDau);
    console.log("Tên loại hình:", match.loaiHinhThiDau.name);

    // Danh sách các loại hình thi đấu
    const tabMapping = {
        'Đối Kháng': 'combat',
        'Quyền Pháp': 'performance',
        'Binh Khí': 'weapon',
        'Đồng Đội': 'team'
    };

    // Lấy tab tương ứng, mặc định là combat nếu không tìm thấy
    const activeTabName = tabMapping[match.loaiHinhThiDau.name] || 'combat';

    // Kích hoạt tab
    const activeTab = document.querySelector(`.tab[data-tab="${activeTabName}"]`);
    const activeContent = document.getElementById(`${activeTabName}-scoring`);

    if (activeTab) {
        activeTab.classList.remove('disabled');
        activeTab.classList.add('active');
        activeTab.style.pointerEvents = 'auto'; // Cho phép click
        activeTab.style.opacity = '1'; // Hiển thị bình thường
    }

    if (activeContent) {
        activeContent.classList.add('active');
        activeContent.style.display = 'block';
    }

    // Điền thông tin vận động viên nếu có
    if (match.vanDongVien1 && match.vanDongVien2) {
        // Điền thông tin cho tab Đối kháng
        if (activeTabName === 'combat') {
            document.querySelector('.athlete-card .athlete-header.blue-header .athlete-name').textContent = match.vanDongVien1.hoTen;
            document.querySelector('.athlete-card .athlete-header.blue-header .athlete-club').textContent = match.vanDongVien1.clb;

            document.querySelector('.athlete-card .athlete-header.red-header .athlete-name').textContent = match.vanDongVien2.hoTen;
            document.querySelector('.athlete-card .athlete-header.red-header .athlete-club').textContent = match.vanDongVien2.clb;

            // Điền thông tin chi tiết nếu có
            const blueInfoItems = document.querySelectorAll('.athlete-card:first-child .info-item .detail-value');
            const redInfoItems = document.querySelectorAll('.athlete-card:last-child .info-item .detail-value');

            if (blueInfoItems.length >= 4) {
                blueInfoItems[0].textContent = match.vanDongVien1.tuoi || 'N/A';
                blueInfoItems[1].textContent = match.vanDongVien1.canNang ? `${match.vanDongVien1.canNang} kg` : 'N/A';
                blueInfoItems[2].textContent = match.vanDongVien1.chieuCao ? `${match.vanDongVien1.chieuCao} cm` : 'N/A';
                blueInfoItems[3].textContent = match.vanDongVien1.tySoThang ? `${match.vanDongVien1.tySoThang}` : 'N/A';
            }

            if (redInfoItems.length >= 4) {
                redInfoItems[0].textContent = match.vanDongVien2.tuoi || 'N/A';
                redInfoItems[1].textContent = match.vanDongVien2.canNang ? `${match.vanDongVien2.canNang} kg` : 'N/A';
                redInfoItems[2].textContent = match.vanDongVien2.chieuCao ? `${match.vanDongVien2.chieuCao} cm` : 'N/A';
                redInfoItems[3].textContent = match.vanDongVien2.tySoThang ? `${match.vanDongVien2.tySoThang}` : 'N/A';
            }
        }
        // Thêm điền thông tin cho các tab khác nếu cần
        else if (activeTabName === 'performance') {
            document.querySelector('.performance-header .athlete-name').textContent = match.vanDongVien1.hoTen;
            document.querySelector('.performance-header .athlete-club').textContent = match.vanDongVien1.clb;
        }
    }

    // Thêm sự kiện click cho các tab
    tabs.forEach(tab => {
        tab.addEventListener('click', function () {
            // Chỉ cho phép chuyển tab nếu không bị disabled
            if (!this.classList.contains('disabled')) {
                // Loại bỏ active khỏi tất cả các tab
                tabs.forEach(t => {
                    t.classList.remove('active');
                    t.style.opacity = '0.5';
                    t.style.pointerEvents = 'none';
                });

                // Ẩn tất cả nội dung tab
                tabContents.forEach(content => {
                    content.classList.remove('active');
                    content.style.display = 'none';
                });

                // Kích hoạt tab được chọn
                this.classList.add('active');
                this.style.opacity = '1';
                this.style.pointerEvents = 'auto';

                const tabName = this.getAttribute('data-tab');
                const activeContent = document.getElementById(`${tabName}-scoring`);

                if (activeContent) {
                    activeContent.classList.add('active');
                    activeContent.style.display = 'block';
                }
            }
        });
    });

    console.log('Đã thiết lập UI cho trận đấu:', match);
}



//xuly