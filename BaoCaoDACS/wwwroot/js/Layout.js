


    const baseUrlLayout = (window.appBase || '/');
    const normalizedBaseUrlLayout = baseUrlLayout.endsWith('/') ? baseUrlLayout : `${baseUrlLayout}/`;

    let activeTournamentId = null;

      
     function showRegisterModal(event, tournamentId) {
        event.stopPropagation();
        const modal = document.getElementById('register-tournament-modal');
        modal.dataset.tournamentId = tournamentId;
        modal.style.display = 'block';
    }


        function closeModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';  // Ẩn modal
        } else {
            console.error("Không tìm thấy modal:", modalId);
        }
    }

     // Gắn sự kiện
      document.addEventListener('DOMContentLoaded', () => {
        const modalClose = document.querySelector('#register-tournament-modal .modal-close');
        if (modalClose) {
            modalClose.addEventListener('click', () => {
                const modal = document.getElementById('register-tournament-modal');
                modal.style.display = 'none';
                document.body.classList.remove('modal-open');
            });
        }
      });
       
    //ham gửi dữ liệu đăng kí
    document.querySelector('#register-tournament-modal form').addEventListener('submit', async function (e) {
        e.preventDefault();

        const tournamentId = document.querySelector('#register-tournament-modal').dataset.tournamentId;
        const formData = {
            ParticipantID: `PART-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
            FullName: document.getElementById('register-name').value,
            Club: document.getElementById('register-club').value || "Không có",
            sdt: document.getElementById('register-phone').value,
            email: document.getElementById('register-email').value,
            TournamentID: parseInt(tournamentId, 10),
            Score: 0
        };

       
        if (!formData.FullName || !formData.TournamentID) {
            Swal.fire('Lỗi', 'Vui lòng nhập họ tên và chọn giải.', 'warning');
            return;
        }

        try {
            
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const headers = { 'Content-Type': 'application/json' };
            if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;

            const resp = await fetch(`${normalizedBaseUrlLayout}Home/PostParticipant`, {
                method: 'POST',
                headers,
                credentials: 'same-origin',
                body: JSON.stringify(formData)
            });

            const text = await resp.text(); 
            let payload = null;
            try { payload = text ? JSON.parse(text) : null; } catch { payload = text; }

            console.log('POST /Home/PostParticipant =>', resp.status, payload);

            if (!resp.ok) {
               
                let msg = 'Đã có lỗi xảy ra.';
                if (payload) {
                    if (typeof payload === 'string') msg = payload;
                    else if (payload.Message) msg = payload.Message;
                    else if (payload.detail) msg = payload.detail || payload.Detail || JSON.stringify(payload);
                    else msg = JSON.stringify(payload);
                } else {
                    msg = `HTTP ${resp.status}`;
                }

                Swal.fire({
                    title: 'Đăng ký thất bại',
                    html: `<pre style="white-space:pre-wrap">${escapeHtml(msg)}</pre>`,
                    icon: 'error'
                });
                return;
            }
            Swal.fire({
                title: 'Thành công!',
                text: 'Đăng ký thành công!',
                icon: 'success',
                confirmButtonText: 'OK'
            }).then(() => {
                closeModal('register-tournament-modal');
                resetForm();
                // optionally refresh list: location.reload();
            });

        } catch (err) {
            console.error('Fetch error:', err);
            Swal.fire('Lỗi hệ thống', escapeHtml(err.message || 'Không thể kết nối tới server'), 'error');
        }
    });

// helper to avoid HTML injection when showing server text
function escapeHtml(s) {
    if (!s) return '';
    return s.replace(/[&<>"'`=\/]/g, function (c) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '/': '&#x2F;', '`': '&#x60;', '=': '&#x3D;' }[c];
    });
}


    // Hàm hỗ trợ
     function handleRegistrationError(error) {
        const errorMessages = {
            400: 'Dữ liệu không hợp lệ',
            401: 'Yêu cầu đăng nhập',
            404: 'API endpoint không tồn tại',
            500: 'Lỗi hệ thống'
        };

        alert(errorMessages[error.status] || `Lỗi không xác định: ${error.message}`);
    }
       function resetForm() {
        const form = document.getElementById('register-tournament-modal').querySelector('form');
        if(form) form.reset();
    }
  

 
  

    // Hàm hỗ trợ để lấy class trạng thái
    function getStatusClass(status) {
        switch (status) {
            case "Upcoming":
                return "bg-primary";
            case "Ongoing":
                return "bg-success";
            case "Completed":
                return "bg-secondary";
            case "Cancelled":
                return "bg-danger";
            default:
                return "bg-primary";
        }
    }

    // Hàm hỗ trợ để lấy text trạng thái
    function getStatusText(status) {
        switch (status) {
            case "Upcoming":
                return "Sắp diễn ra";
            case "Ongoing":
                return "Đang diễn ra";
            case "Completed":
                return "Đã kết thúc";
            case "Cancelled":
                return "Đã hủy";
            default:
                return status;
        }
    }
    //thanhcong
   



//gọi render touterments
    fetch(`${normalizedBaseUrlLayout}Home/GetTournaments`)
        .then(response => response.json())
        .then(data => {
            // Xử lý dữ liệu
            const now = new Date();
            const promises = data.map(tournament => {
                console.log(tournament);
                let actionButtonHtml;
                const isEnded = new Date(tournament.endDate) < now;
                if (isAuthenticated) {
                    if (isEnded) {
                        actionButtonHtml = `
                        <button class="btn btn-secondary w-100" disabled title="Giải đấu đã kết thúc">
                            Đã Kết Thúc
                        </button>`;
                    } else {
                        actionButtonHtml = `
                        <button class="btn btn-primary w-100" onclick="showRegisterModal(event, ${tournament.tournamentID})">
                            Đăng Ký Tham Gia
                        </button>`;
                    }
                } else {
                    if (isEnded) {
                        actionButtonHtml = `
                        <button class="btn btn-secondary w-100" disabled title="Giải đấu đã kết thúc">
                            Đã Kết Thúc
                        </button>`;
                    } else {
                        actionButtonHtml = `
                        <button class="btn btn-secondary w-100" disabled title="Vui lòng đăng nhập để đăng ký">
                            Đăng Ký Tham Gia
                        </button>`;
                    }
                }
                // Đẩy dữ liệu lên web
                var html = `
                         <div class="card tournament-card h-100" data-tournament-id="${tournament.tournamentID}">
                            <!-- Hình ảnh giải đấu -->
                            <div class="tournament-img card-img-top" style="height: 200px; background-image: url('${tournament.imageUrl}'); background-size: cover; background-position: center;"></div>
                            <div class="card-body d-flex flex-column">
                                <!-- Trạng thái giải đấu -->
                                <span class="badge ${getStatusClass(tournament.status)} mb-2 align-self-start">${getStatusText(tournament.status)}</span>
                                <!-- Tên giải đấu -->
                                <h5 class="card-title">${tournament.name}</h5>
                                                           
                                     <!-- Bộ đếm ngược -->
                             <div class="countdown-group">
                                <div class="countdown-container mb-3">
                                    <div class="countdown d-flex justify-content-between">
                                        <div class="countdown-item text-center px-1">
                                            <div class="countdown-value fw-bold fs-5 days">00</div>
                                            <div class="countdown-label small text-muted">Ngày</div>
                                        </div>
                                        <div class="countdown-item text-center px-1">
                                            <div class="countdown-value fw-bold fs-5 hours" >00</div>
                                            <div class="countdown-label small text-muted">Giờ</div>
                                        </div>
                                        <div class="countdown-item text-center px-1">
                                            <div class="countdown-value fw-bold fs-5 minutes" >00</div>
                                            <div class="countdown-label small text-muted">Phút</div>
                                        </div>
                                        <div class="countdown-item text-center px-1">
                                            <div class="countdown-value fw-bold fs-5 seconds" >00</div>
                                            <div class="countdown-label small text-muted">Giây</div>
                                        </div>
                                    </div>
                                </div>

                      
                                 <div class="tournament-meta mb-3">
                                    <div class="d-flex align-items-center mb-2">
                                        <i class="bi bi-calendar3 me-2 text-muted"></i>
                                            <span class="event-date">${new Date(tournament.startDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })} - ${new Date(tournament.endDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })}</span>
                                    </div>
                                    <div class="d-flex align-items-center">
                                        <i class="bi bi-people me-2 text-muted" id="number-of-participants-${tournament.tournamentID}"></i>
                                        <span>${tournament.quyMoiaiDa}</span>
                                    </div>
                                </div>
                            </div>
                           
                                <!-- Địa điểm -->
                                <div class="tournament-location mb-3">
                                    <div class="d-flex">
                                        <i class="bi bi-geo-alt me-2 text-muted"></i>
                                        <div class="location-text small">${tournament.location}</div>
                                    </div>
                                </div>
                                <!-- Nút xem chi tiết - tự động đẩy xuống dưới cùng -->
                                <div class="mt-auto pt-2">
                                          
                                       ${actionButtonHtml}                                
                                 
                                </div>
                            </div>
                        </div>
                    `;
                const cardElement = document.createElement('div');
                cardElement.innerHTML = html;
                document.getElementById("tournaments").appendChild(cardElement);
                new Countdown(cardElement.querySelector('.countdown-container'), tournament.endDate);
          
                return fetch(`${normalizedBaseUrlLayout}Home/GetNumberOfParticipants/${tournament.tournamentID}`)
                    .then(response => response.json())
                    .then(data => {
                        cardElement.querySelector(`#number-of-participants-${tournament.tournamentID}`).innerHTML = `
                            <i class="bi bi-people me-2 text-muted"></i>
                            <span>${data.count} VĐV</span>
                        `;
                  
                    });
            });
            return Promise.all(promises);
        })
        .then(() => {
            
        })
        .catch(error => console.error(error));



// Bộ đếm ngược

class Countdown {
    constructor(container, endDateString) {
        this.container = container;
        this.endDate = this.parseEndDate(endDateString);
        this.elements = {
            days: container.querySelector('.days'),
            hours: container.querySelector('.hours'),
            minutes: container.querySelector('.minutes'),
            seconds: container.querySelector('.seconds')
        };
        this.interval = null;
        this.init();
    }

    parseEndDate(endDateString) {
        // Sử dụng trực tiếp endDate từ API thay vì parse từ HTML
        const date = new Date(endDateString);
        date.setHours(23, 59, 59, 0); // Đặt thời gian kết thúc là cuối ngày
        return date;
    }

    init() {
        this.update();
        this.interval = setInterval(() => this.update(), 1000);
    }

    update() {
        const now = new Date();
        const diff = this.endDate - now;

        if (diff <= 0) {
            this.container.innerHTML = `<div class="alert alert-warning p-2 text-center">Sự kiện đã kết thúc</div>`;
            clearInterval(this.interval);
            return;
        }

        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);

        this.elements.days.textContent = days.toString().padStart(2, '0');
        this.elements.hours.textContent = hours.toString().padStart(2, '0');
        this.elements.minutes.textContent = minutes.toString().padStart(2, '0');
        this.elements.seconds.textContent = seconds.toString().padStart(2, '0');
    }
}

//xu ly thang nam
function showAlert(message, type = 'success') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} fixed-top mx-auto mt-3`;
    alertDiv.style.width = '300px';
    alertDiv.textContent = message;
    document.body.appendChild(alertDiv);
    setTimeout(() => alertDiv.remove(), 3000);
}
const monthNames = [
    "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
    "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
];
const dayNames = ["CN", "T2", "T3", "T4", "T5", "T6", "T7"];
const calendarGrid = document.querySelector(".calendar-grid");
const currentMonthDisplay = document.getElementById("current-month");
let currentMonth = new Date().getMonth();
let currentYear = new Date().getFullYear();
const today = new Date(); // Di chuyển ra ngoài để chỉ khởi tạo 1 lần

function renderCalendar(month, year) {
    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    calendarGrid.innerHTML = dayNames.map(day => `<div class="day-name">${day}</div>`).join("");

    // Thêm các ô trống cho những ngày trước ngày đầu tiên của tháng
    for (let i = 0; i < firstDay; i++) {
        calendarGrid.innerHTML += `<div class="calendar-day empty"></div>`;
    }

    // Thêm các ngày trong tháng
    for (let day = 1; day <= daysInMonth; day++) {
        const isToday =
            day === today.getDate() &&
            month === today.getMonth() &&
            year === today.getFullYear();

        calendarGrid.innerHTML += `
          <div class="calendar-day ${isToday ? "current-day" : ""}">
            <span class="day-number">${day}</span>
          </div>`;
    }
}

function updateMonthDisplay() {
    currentMonthDisplay.textContent = `${monthNames[currentMonth]}, ${currentYear}`;
}

// Xử lý nút tháng trước
document.getElementById("prev-month").addEventListener("click", () => {
    currentMonth--;
    if (currentMonth < 0) {
        currentMonth = 11;
        currentYear--;
    }
    updateMonthDisplay();
    renderCalendar(currentMonth, currentYear);
});

// Xử lý nút tháng sau
document.getElementById("next-month").addEventListener("click", () => {
    currentMonth++;
    if (currentMonth > 11) {
        currentMonth = 0;
        currentYear++;
    }
    updateMonthDisplay();
    renderCalendar(currentMonth, currentYear);
});

// Khởi tạo ban đầu
updateMonthDisplay();
renderCalendar(currentMonth, currentYear);
