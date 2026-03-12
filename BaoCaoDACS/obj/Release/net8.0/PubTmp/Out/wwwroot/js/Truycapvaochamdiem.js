

// them nguoi tham gia
const baseUrlAccess = (window.appBase || '/');
const normalizedBaseUrlAccess = baseUrlAccess.endsWith('/') ? baseUrlAccess : `${baseUrlAccess}/`;

function fetchMatchParticipants(match) {

    return fetch(`${normalizedBaseUrlAccess}ChamDiem/GetMatchParticipants?matchId=${encodeURIComponent(match.matchId)}`)
        .then(response => {
            console.log('Response status:', response.status);
            console.log('Response headers:', Object.fromEntries(response.headers.entries()));
            if (!response.ok) {
                return response.text().then(text => {
                    console.error('Error response text:', text);
                    throw new Error(`Không thể tải thông tin vận động viên: ${text}`);
                });
            }
            return response.json();
        })
        .then(data => {
            console.log('Dữ liệu nhận được:', data);
            const tabMapping = {
                'Đối Kháng': {
                    selector: '.athlete-cards-container',
                    renderFunction: renderCombatAthletes
                },
                'Quyền Pháp': {
                    selector: '#performance-scoring',
                    renderFunction: renderPerformanceAthlete
                },
                'Binh Khí': {
                    selector: '#weapon-scoring .performance-header',
                    renderFunction: renderWeaponAthlete
                },
                'Đồng Đội': {
                    selector: '#team-scoring .performance-header',
                    renderFunction: renderTeamAthlete
                }
            };

            const matchType = match.loaiHinhThiDau.name;
            const tabConfig = tabMapping[matchType] || tabMapping['Đối Kháng'];

            if (matchType === 'Đối Kháng') {
                const blueContainer = document.querySelector('.athlete-cards-1-container');
                const redContainer = document.querySelector('.athlete-cards-2-container');

                const blueAthleteCard = createAthleteCard(data.vanDongVien1, 'blue');
                const redAthleteCard = createAthleteCard(data.vanDongVien2, 'red');

                blueContainer.innerHTML = blueAthleteCard;
                redContainer.innerHTML = redAthleteCard;
            } else {
                const container = document.querySelector(tabConfig.selector);
                if (container) {
                    tabConfig.renderFunction(data, container);
                }
            }
        })
        .catch(error => {
            console.error('Full error details:', {
                message: error.message,
                stack: error.stack,
                name: error.name
            });
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi kết nối',
                    text: 'Không thể kết nối đến máy chủ',
                    confirmButtonText: 'Thử lại'
                }).then(() => window.location.reload());
            } else {
                alert('Lỗi hệ thống! Vui lòng tải lại trang');
            }
        });
}

// Đảm bảo hàm có sẵn ở phạm vi global cho các script khác
window.fetchMatchParticipants = fetchMatchParticipants;
function renderCombatAthletes(data, container) {
    const blueAthleteCard = createAthleteCard(data.vanDongVien1, 'blue');
    const redAthleteCard = createAthleteCard(data.vanDongVien2, 'red');

    container.innerHTML = `
                  <div class="athlete-card-">${blueAthleteCard}</div>
                  <div class="athlete-card">${redAthleteCard}</div>
              `;
}

function renderPerformanceAthlete(data, container) {
    console.log('Dữ liệu VĐV:', data);
    const athlete = data.vanDongVien1; // Lấy thông tin VĐV
    console.log('Thông tin athlete:', athlete);
    const performanceAthleteHtml = createPerformanceAthlete(athlete, 'performance');
    container.innerHTML = performanceAthleteHtml;
    requestAnimationFrame(() => {
        initPerformanceScoring()

    });
}

function renderWeaponAthlete(data, container) {
    const athlete = data.vanDongVien1; //  VĐV cho nội dung binh khí
    container.innerHTML = `
                      <h3 class="athlete-name">${athlete.hoTen || 'Chưa xác định'}</h3>
                      <span class="athlete-club">${athlete.clb || 'Chưa xác định'}</span>
                  `;
}

function renderTeamAthlete(data, container) {
    // Đối với đồng đội, có thể là thông tin đội
    container.innerHTML = `
                      <h3 class="athlete-name">${data.tenDoi || 'Chưa xác định'}</h3>
                      <span class="athlete-club">${data.clb || 'Chưa xác định'}</span>
                  `;
}
function createAthleteCard(athlete, side) {
    return `
              <div class="athlete-header ${side}-header">
                  <h3 class="athlete-name">${athlete.hoTen || 'Chưa xác định'}</h3>
                  <span class="athlete-club">${athlete.clb || 'Chưa xác định'}</span>
              </div>
              <div class="athlete-info">
                  <div class="info-grid">
                      <div class="info-item">
                          <span class="detail-label">Tuổi</span>
                          <span class="detail-value">${athlete.tuoi || 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Cân nặng</span>
                          <span class="detail-value">${athlete.canNang ? `${athlete.canNang} kg` : 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Chiều cao</span>
                          <span class="detail-value">${athlete.chieuCao ? `${athlete.chieuCao} cm` : 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Số trận thắng/thua</span>
                          <span class="detail-value">${athlete.soTranThang}/${athlete.soTranThua}</span>
                      </div>
                  </div>
              </div>
               <div class="athlete-score">
                              <div class="score-total ${side}-score" id="${side}-score">0</div>
                              <div class="scoring-buttons">
                                  <button class="score-btn ${side}-btn" data-value="1" data-target="${side}">+1</button>
                                  <button class="score-btn ${side}-btn" data-value="2" data-target="${side}">+2</button>
                                  <button class="score-btn ${side}-btn" data-value="3" data-target="${side}">+3</button>
                                  <button class="score-btn penalty-btn" data-value="-1" data-target="${side}">-1</button>
                                  <button class="score-btn penalty-btn" data-value="-2" data-target="${side}">-2</button>
                                  <button class="score-btn penalty-btn" data-value="caution" data-target="${side}">Cảnh cáo</button>
                              </div>
                          </div>
          `;
}
function createPerformanceAthlete(athlete, side) {

    return `
                  <div class="performance-header">
                          <h3 class="athlete-name">${athlete.hoTen}</h3>
                          <span class="athlete-club">CLB ${athlete.clb || 'N/A'}</span>
                      </div>
                      <div class="performance-body">
                          <div class="criteria-grid">
                              <div class="criteria-name">Kỹ thuật biểu diễn<br><span style='font-size:0.9em'>(Demonstration Technique)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-tech-score">-</button>
                                  <div class="score-display" id="perf-tech-score">5.0</div>
                                  <button class="score-inc" data-target="perf-tech-score">+</button>
                              </div>
                          </div>
                          <div class="criteria-grid">
                              <div class="criteria-name">Sức mạnh/Nhịp điệu/Điểm dừng KT/Tốc độ<br><span style='font-size:0.9em'>(Power/Movement Rhythm/Technical Pause/Speed)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-power-score">-</button>
                                  <div class="score-display" id="perf-power-score">3.0</div>
                                  <button class="score-inc" data-target="perf-power-score">+</button>
                              </div>
                          </div>
                          <div class="criteria-grid">
                              <div class="criteria-name">Thần thái/Thần khí<br><span style='font-size:0.9em'>(Soulfulness)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-spirit-score">-</button>
                                  <div class="score-display" id="perf-spirit-score">2.0</div>
                                  <button class="score-inc" data-target="perf-spirit-score">+</button>
                              </div>
                          </div>
                          <div class="deduction-section">
                              <h4>Trừ điểm lỗi (Penalties)</h4>
                              <div class="deduction-grid">
                                  <div class="deduction-item">
                                      <span class="deduction-name">Sai kỹ thuật<br><span style='font-size:0.9em'>(Faulty Technique)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="faulty" data-target="perf-faulty">-0.2</button>
                                          <span id="perf-faulty-count">0</span>
                                      </div>
                                  </div>
                                  <div class="deduction-item">
                                      <span class="deduction-name">Thừa thiếu động tác<br><span style='font-size:0.9em'>(Left/Surplus Movement)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="surplus" data-target="perf-surplus">-0.2</button>
                                          <span id="perf-surplus-count">0</span>
                                      </div>
                                  </div>
                                  <div class="deduction-item">
                                      <span class="deduction-name">Thăng bằng/Chạm đất<br><span style='font-size:0.9em'>(Balance/Ground-touching)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="balance" data-target="perf-balance">-0.2</button>
                                          <span id="perf-balance-count">0</span>
                                      </div>
                                  </div>
                              </div>
                          </div>
                          <div class="final-score-section">
                              <div class="final-score-title">Tổng điểm cuối cùng (Total Score)</div>
                              <div class="final-score-value" id="final-performance-score">10.0</div>
                          </div>
                      </div>
          `;
} const today = new Date();
const formattedDate = today.toLocaleDateString('vi-VN');


// Hàm tạo thẻ section từ 1 trận đấu
function createMatchHtml(match) {
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
            window.location.href = `${normalizedBaseUrlAccess}Home/Index`; // Điều hướng về trang chủ
        });
        return;
    }

    // Gọi API với matchId cụ thể
    fetch(`${normalizedBaseUrlAccess}ChamDiem/GetMatch?matchId=${encodeURIComponent(matchId)}`)
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

            setupMatchUI(match);
            fetchMatchParticipants(match);

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
        // Tương tự cho weapon và team
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


// them nguoi tham gia

function fetchMatchParticipants(match) {

    return fetch(`${normalizedBaseUrlAccess}ChamDiem/GetMatchParticipants?matchId=${encodeURIComponent(match.matchId)}`)
        .then(response => {
            console.log('Response status:', response.status);
            console.log('Response headers:', Object.fromEntries(response.headers.entries()));
            if (!response.ok) {
                return response.text().then(text => {
                    console.error('Error response text:', text);
                    throw new Error(`Không thể tải thông tin vận động viên: ${text}`);
                });
            }
            return response.json();
        })
        .then(data => {
            console.log('Dữ liệu nhận được:', data);
            const tabMapping = {
                'Đối Kháng': {
                    selector: '.athlete-cards-container',
                    renderFunction: renderCombatAthletes
                },
                'Quyền Pháp': {
                    selector: '#performance-scoring-card',
                    renderFunction: renderPerformanceAthlete
                },
                'Binh Khí': {
                    selector: '#performance-weapon-scoring',
                    renderFunction: renderWeaponAthlete
                },
                'Đồng Đội': {
                    selector: '#performance-team-scoring',
                    renderFunction: renderTeamAthlete
                }
            };

            const matchType = match.loaiHinhThiDau.name;
            const tabConfig = tabMapping[matchType] || tabMapping['Đối Kháng'];

            if (matchType === 'Đối Kháng') {
                const blueContainer = document.querySelector('.athlete-cards-1-container');
                const redContainer = document.querySelector('.athlete-cards-2-container');

                const blueAthleteCard = createAthleteCard(data.vanDongVien1, 'blue');
                const redAthleteCard = createAthleteCard(data.vanDongVien2, 'red');

                blueContainer.innerHTML = blueAthleteCard;
                redContainer.innerHTML = redAthleteCard;


                blueParticipantId = data.vanDongVien1.participantId;
                redParticipantId = data.vanDongVien2.participantId;
                currentMatchId = data.vanDongVien1.matchid;
                console.log('Dữ liệu nhận khác:', currentMatchId);
                console.log('Dữ liệu nhận khác:', blueParticipantId);
                console.log('Dữ liệu nhận khác:', redParticipantId);
            } else {
                const container = document.querySelector(tabConfig.selector);
                if (container) {
                    tabConfig.renderFunction(data, container);
                }
            }
        })
        .catch(error => {
            console.error('Full error details:', {
                message: error.message,
                stack: error.stack,
                name: error.name
            });
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi kết nối',
                    text: 'Không thể kết nối đến máy chủ',
                    confirmButtonText: 'Thử lại'
                }).then(() => window.location.reload());
            } else {
                alert('Lỗi hệ thống! Vui lòng tải lại trang');
            }
        });
}

function renderCombatAthletes(data, container) {
    const blueAthleteCard = createAthleteCard(data.vanDongVien1, 'blue');
    const redAthleteCard = createAthleteCard(data.vanDongVien2, 'red');

    container.innerHTML = `
                  <div class="athlete-card-">${blueAthleteCard}</div>
                  <div class="athlete-card">${redAthleteCard}</div>
              `;
}
//Quyền pháp
function renderPerformanceAthlete(data, container) {
    console.log('Dữ liệu VĐV:', data);
    const athlete = data.vanDongVien1; // Lấy thông tin VĐV
    console.log('Thông tin athlete:', athlete);
    const performanceAthleteHtml = createPerformanceAthlete(athlete, 'performance');
    container.innerHTML = performanceAthleteHtml;
    blueParticipantId = data.vanDongVien1.participantId;
    currentMatchId = data.vanDongVien1.matchid;
    console.log('Dữ liệu nhận khác:', currentMatchId);
    console.log('Dữ liệu nhận khác:', blueParticipantId);


    // Đợi một khung hình để chắc chắn DOM đã render
    requestAnimationFrame(() => {
        initPerformanceScoring()

    });
}
//BInh khí
function renderWeaponAthlete(data, container) {
    const athlete = data.vanDongVien1; //  VĐV cho nội dung binh khí
    console.log('Thông tin athlete:', athlete);
    const performanceAthleteHtml = createweaponAthlete(athlete, 'weapon');
    container.innerHTML = performanceAthleteHtml;


    blueParticipantId = data.vanDongVien1.participantId;
    currentMatchId = data.vanDongVien1.matchid;
    console.log('Dữ liệu nhận khác:', currentMatchId);
    console.log('Dữ liệu nhận khác:', blueParticipantId);

    requestAnimationFrame(() => {

        initWeaponScoring()

    });

}
//chấm điểm đồng đội
function renderTeamAthlete(data, container) {
    // thông tin đội
    const athlete = data.vanDongVien1;
    console.log('Thông tin athlete:', athlete);
    const performanceAthleteHtml = creatteamAthlete(athlete, 'team');
    container.innerHTML = performanceAthleteHtml;


    blueParticipantId = data.vanDongVien1.participantId;
    currentMatchId = data.vanDongVien1.matchid;
    console.log('Dữ liệu nhận khác:', currentMatchId);
    console.log('Dữ liệu nhận khác:', blueParticipantId);
    requestAnimationFrame(() => {

        initTeamScoring()

    });
}
function createAthleteCard(athlete, side) {
    return `
              <div class="athlete-header ${side}-header">
                  <h3 class="athlete-name">${athlete.hoTen || 'Chưa xác định'}</h3>
                  <span class="athlete-club">${athlete.clb || 'Chưa xác định'}</span>
              </div>
              <div class="athlete-info">
                  <div class="info-grid">
                      <div class="info-item">
                          <span class="detail-label">Tuổi</span>
                          <span class="detail-value">${athlete.tuoi || 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Cân nặng</span>
                          <span class="detail-value">${athlete.canNang ? `${athlete.canNang} kg` : 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Chiều cao</span>
                          <span class="detail-value">${athlete.chieuCao ? `${athlete.chieuCao} cm` : 'N/A'}</span>
                      </div>
                      <div class="info-item">
                          <span class="detail-label">Số trận thắng/thua</span>
                          <span class="detail-value">${athlete.soTranThang}/${athlete.soTranThua}</span>
                      </div>
                  </div>
              </div>
               <div class="athlete-score">
                              <div class="score-total ${side}-score" id="${side}-score">0</div>
                              <div class="scoring-buttons">
                                  <button class="score-btn ${side}-btn" data-value="1" data-target="${side}">+1</button>
                                  <button class="score-btn ${side}-btn" data-value="2" data-target="${side}">+2</button>
                                  <button class="score-btn ${side}-btn" data-value="3" data-target="${side}">+3</button>
                                  <button class="score-btn penalty-btn" data-value="-1" data-target="${side}">-1</button>
                                  <button class="score-btn penalty-btn" data-value="-2" data-target="${side}">-2</button>
                                  <button class="score-btn penalty-btn" data-value="caution" data-target="${side}">Cảnh cáo</button>
                              </div>
                          </div>
          `;
}
function createPerformanceAthlete(athlete, side) {

    return `
                  <div class="performance-header">
                          <h3 class="athlete-name">${athlete.hoTen}</h3>
                          <span class="athlete-club">CLB ${athlete.clb || 'N/A'}</span>
                      </div>
                      <div class="performance-body">
                          <div class="criteria-grid">
                              <div class="criteria-name">Kỹ thuật biểu diễn<br><span style='font-size:0.9em'>(Demonstration Technique)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-tech-score">-</button>
                                  <div class="score-display" id="perf-tech-score">5.0</div>
                                  <button class="score-inc" data-target="perf-tech-score">+</button>
                              </div>
                          </div>
                          <div class="criteria-grid">
                              <div class="criteria-name">Sức mạnh/Nhịp điệu/Điểm dừng KT/Tốc độ<br><span style='font-size:0.9em'>(Power/Movement Rhythm/Technical Pause/Speed)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-power-score">-</button>
                                  <div class="score-display" id="perf-power-score">3.0</div>
                                  <button class="score-inc" data-target="perf-power-score">+</button>
                              </div>
                          </div>
                          <div class="criteria-grid">
                              <div class="criteria-name">Thần thái/Thần khí<br><span style='font-size:0.9em'>(Soulfulness)</span></div>
                              <div class="criteria-score">
                                  <button class="score-dec" data-target="perf-spirit-score">-</button>
                                  <div class="score-display" id="perf-spirit-score">2.0</div>
                                  <button class="score-inc" data-target="perf-spirit-score">+</button>
                              </div>
                          </div>
                          <div class="deduction-section">
                              <h4>Trừ điểm lỗi (Penalties)</h4>
                              <div class="deduction-grid">
                                  <div class="deduction-item">
                                      <span class="deduction-name">Sai kỹ thuật<br><span style='font-size:0.9em'>(Faulty Technique)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="faulty" data-target="perf-faulty">-0.2</button>
                                          <span id="perf-faulty-count">0</span>
                                      </div>
                                  </div>
                                  <div class="deduction-item">
                                      <span class="deduction-name">Thừa thiếu động tác<br><span style='font-size:0.9em'>(Left/Surplus Movement)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="surplus" data-target="perf-surplus">-0.2</button>
                                          <span id="perf-surplus-count">0</span>
                                      </div>
                                  </div>
                                  <div class="deduction-item">
                                      <span class="deduction-name">Thăng bằng/Chạm đất<br><span style='font-size:0.9em'>(Balance/Ground-touching)</span></span>
                                      <div>
                                          <button class="deduction-btn" data-deduction="-0.2" data-type="balance" data-target="perf-balance">-0.2</button>
                                          <span id="perf-balance-count">0</span>
                                      </div>
                                  </div>
                              </div>
                          </div>
                          <div class="final-score-section">
                              <div class="final-score-title">Tổng điểm cuối cùng (Total Score)</div>
                              <div class="final-score-value" id="final-performance-score">10.0</div>
                          </div>
                      </div>
          `;

}


function createweaponAthlete(athlete, side) {

    return `<div class="performance-header">
                            <h3 class="athlete-name">${athlete.hoTen} - Binh khí </h3>
                            <span class="athlete-club">CLB ${athlete.clb || 'N/A'}</span>
                        </div>
                        <div class="performance-body">
                            <div class="criteria-grid">
                                <div class="criteria-name">Kỹ thuật biểu diễn/Sử dụng binh khí <br><span style='font-size:0.9em'>(Performance/Weapon Use)</span></div>
                                <div class="criteria-score">
                                    <button class="score-dec" data-target="weapon-tech-score">-</button>
                                    <div class="score-display" id="weapon-tech-score">5.0</div>
                                    <button class="score-inc" data-target="weapon-tech-score">+</button>
                                </div>
                            </div>
                            <div class="criteria-grid">
                                <div class="criteria-name">Kỹ năng sử dụng vũ khí<br><span style='font-size:0.9em'>(Weapon Skill)</span></div>
                                <div class="criteria-score">
                                    <button class="score-dec" data-target="weapon-skill-score">-</button>
                                    <div class="score-display" id="weapon-skill-score">3.0</div>
                                    <button class="score-inc" data-target="weapon-skill-score">+</button>
                                </div>
                            </div>
                            <div class="criteria-grid">
                                <div class="criteria-name">Độ khó kỹ thuật<br><span style='font-size:0.9em'>(Technical Difficulty)</span></div>
                                <div class="criteria-score">
                                    <button class="score-dec" data-target="weapon-difficulty-score">-</button>
                                    <div class="score-display" id="weapon-difficulty-score">2.0</div>
                                    <button class="score-inc" data-target="weapon-difficulty-score">+</button>
                                </div>
                            </div>
                            <div class="deduction-section">
                                <h4>Trừ điểm lỗi (Penalties)</h4>
                                <div class="deduction-grid">
                                    <div class="deduction-item">
                                        <span class="deduction-name">Sai kỹ thuật<br><span style='font-size:0.9em'>(Faulty Technique)</span></span>
                                        <div>
                                            <button class="deduction-btn" data-deduction="-0.2" data-type="faulty" data-target="weapon-faulty">-0.2</button>
                                            <span id="weapon-faulty-count">0</span>
                                        </div>
                                    </div>
                                    <div class="deduction-item">
                                        <span class="deduction-name">Thừa thiếu động tác<br><span style='font-size:0.9em'>(Left/Surplus Movement)</span></span>
                                        <div>
                                            <button class="deduction-btn" data-deduction="-0.2" data-type="surplus" data-target="weapon-surplus">-0.2</button>
                                            <span id="weapon-surplus-count">0</span>
                                        </div>
                                    </div>
                                    <div class="deduction-item">
                                        <span class="deduction-name">Thăng bằng/Chạm đất<br><span style='font-size:0.9em'>(Balance/Ground-touching)</span></span>
                                        <div>
                                            <button class="deduction-btn" data-deduction="-0.2" data-type="balance" data-target="weapon-balance">-0.2</button>
                                            <span id="weapon-balance-count">0</span>
                                        </div>
                                    </div>
                                    <div class="deduction-item">
                                        <span class="deduction-name">Binh khí chạm đất<br><span style='font-size:0.9em'>(Weapon dropped)</span></span>
                                        <div>
                                            <button class="deduction-btn" data-deduction="-0.5" data-type="ground" data-target="weapon-ground">-0.5</button>
                                            <span id="weapon-ground-count">0</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="final-score-section">
                                <div class="final-score-title">Tổng điểm cuối cùng (Total Score)</div>
                                <div class="final-score-value" id="final-weapon-score">10.0</div>
                            </div>
                        </div>
          `;

}
function creatteamAthlete(athlete, side) {

    return `<div class="performance-header">
    <h3 class="athlete-name">HUTECH - Biểu diễn đồng đội</h3>
    <span class="athlete-club">CLB ${athlete.clb}</span>
</div>
<div class="performance-body">
    <div class="criteria-grid">
        <div class="criteria-name">Kỹ thuật biểu diễn<br><span style='font-size:0.9em'>(Technique)</span></div>
        <div class="criteria-score">
            <button class="score-dec" data-target="team-tech-score">-</button>
            <div class="score-display" id="team-tech-score">4.0</div>
            <button class="score-inc" data-target="team-tech-score">+</button>
        </div>
    </div>
    <div class="criteria-grid">
        <div class="criteria-name">Phối hợp/Nhịp điệu<br><span style='font-size:0.9em'>(Power/Rhythm)</span></div>
        <div class="criteria-score">
            <button class="score-dec" data-target="team-power-score">-</button>
            <div class="score-display" id="team-power-score">2.0</div>
            <button class="score-inc" data-target="team-power-score">+</button>
        </div>
    </div>
    <div class="criteria-grid">
        <div class="criteria-name">Thẩm mỹ/Thần thái<br><span style='font-size:0.9em'>(Spirit)</span></div>
        <div class="criteria-score">
            <button class="score-dec" data-target="team-spirit-score">-</button>
            <div class="score-display" id="team-spirit-score">2.0</div>
            <button class="score-inc" data-target="team-spirit-score">+</button>
        </div>
    </div>
    <div class="criteria-grid">
        <div class="criteria-name">Điểm sáng tạo<br><span style='font-size:0.9em'>(Creativity)</span></div>
        <div class="criteria-score">
            <button class="score-dec" data-target="team-creativity-score">-</button>
            <div class="score-display" id="team-creativity-score">1.0</div>
            <button class="score-inc" data-target="team-creativity-score">+</button>
        </div>
    </div>
    <div class="criteria-grid">
        <div class="criteria-name">Thời gian biểu diễn<br><span style='font-size:0.9em'>(Time)</span></div>
        <div class="criteria-score">
            <button class="score-dec" data-target="team-time-score">-</button>
            <div class="score-display" id="team-time-score">1.0</div>
            <button class="score-inc" data-target="team-time-score">+</button>
        </div>
    </div>
    <div class="deduction-section">
        <h4>Trừ điểm lỗi (Penalties)</h4>
        <div class="deduction-grid">
            <div class="deduction-item">
                <span class="deduction-name">Sai kỹ thuật<br><span style='font-size:0.9em'>(Faulty Technique)</span></span>
                <div>
                    <button class="deduction-btn" data-deduction="-0.2" data-type="faulty" data-target="team-faulty">-0.2</button>
                    <span id="team-faulty-count">0</span>
                </div>
            </div>
            <div class="deduction-item">
                <span class="deduction-name">Sai nhịp điệu<br><span style='font-size:0.9em'>(Rhythm Error)</span></span>
                <div>
                    <button class="deduction-btn" data-deduction="-0.2" data-type="rhythm" data-target="team-rhythm">-0.2</button>
                    <span id="team-rhythm-count">0</span>
                </div>
            </div>
            <div class="deduction-item">
                <span class="deduction-name">Lố thời gian<br><span style='font-size:0.9em'>(Overtime)</span></span>
                <div>
                    <button class="deduction-btn" data-deduction="-0.2" data-type="overtime" data-target="team-overtime">-0.2</button>
                    <span id="team-overtime-count">0</span>
                </div>
            </div>
            <div class="deduction-item">
                <span class="deduction-name">Lỗi trang phục<br><span style='font-size:0.9em'>(Costume Error)</span></span>
                <div>
                    <button class="deduction-btn" data-deduction="-0.2" data-type="costume" data-target="team-costume">-0.2</button>
                    <span id="team-costume-count">0</span>
                </div>
            </div>
            <div class="deduction-item" id="team-direct-elim-item">
                <span class="deduction-name">Loại trực tiếp<br><span style='font-size:0.9em'>(Direct Elimination)</span></span>
                <div>
                    <button class="deduction-btn" id="team-direct-btn" style="background:#b71c1c;">Loại trực tiếp</button>
                    <span id="team-direct-status" style="color:#b71c1c; font-weight:600; margin-left:8px;"></span>
                </div>
            </div>
        </div>
    </div>
    <div class="final-score-section">
        <div class="final-score-title">Tổng điểm cuối cùng (Total Score)</div>
        <div class="final-score-value" id="final-team-score">10.0</div>
    </div>
</div>
          `;

}