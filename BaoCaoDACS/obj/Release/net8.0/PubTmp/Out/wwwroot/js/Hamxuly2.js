
//chấm điểm bính khí
const baseUrlWeapon = (window.appBase || '/');
const normalizedBaseUrlWeapon = baseUrlWeapon.endsWith('/') ? baseUrlWeapon : `${baseUrlWeapon}/`;

window.initWeaponScoring = function () {
    // Gán sự kiện cho các nút
    attachWeaponScoreEvents();

    // Khởi tạo điểm ban đầu
    updateWeaponFinalScore();

    // Sự kiện submit
    const submitWeaponButton = document.getElementById('submit-weapon');
    if (submitWeaponButton) {
        submitWeaponButton.addEventListener('click', handleSubmitWeaponScore);
    }
}

function handleSubmitWeaponScore() {
    // Lấy thông tin điểm số
    const finalScore = document.getElementById('final-weapon-score')?.textContent || '0.0';
    const techScore = document.getElementById('weapon-tech-score')?.textContent || '0.0';
    const skillScore = document.getElementById('weapon-skill-score')?.textContent || '0.0';
    const difficultyScore = document.getElementById('weapon-difficulty-score')?.textContent || '0.0';

    // Lấy thông tin các lỗi
    const faultyCount = document.getElementById('weapon-faulty-count')?.textContent || '0';
    const surplusCount = document.getElementById('weapon-surplus-count')?.textContent || '0';
    const balanceCount = document.getElementById('weapon-balance-count')?.textContent || '0';
    const groundCount = document.getElementById('weapon-ground-count')?.textContent || '0';

    // Tạo đối tượng dữ liệu để gửi
    const performanceData = {
        finalScore: parseFloat(finalScore),
        matchId: currentMatchId,
        ParticipantId: blueParticipantId,
        danhgia: `Kết quả đánh giá binh khí:
            - Kỹ thuật biểu diễn/Sử dụng binh khí: ${techScore}
            - Kỹ năng sử dụng vũ khí: ${skillScore}
            - Độ khó kỹ thuật: ${difficultyScore}
            - Số lỗi sai kỹ thuật: ${faultyCount}
            - Số lỗi thừa thiếu động tác: ${surplusCount}
            - Số lỗi thăng bằng/chạm đất: ${balanceCount}
            - Số lỗi binh khí chạm đất: ${groundCount}
            Điểm cuối cùng: ${finalScore}`
    };

    // Hiển thị xác nhận trước khi gửi
    Swal.fire({
        title: 'Xác Nhận Gửi Điểm Binh Khí',
        html: `
        <div style="text-align: left; padding: 10px;">
            <p><strong>Kỹ thuật biểu diễn:</strong> ${techScore}</p>
            <p><strong>Kỹ năng sử dụng vũ khí:</strong> ${skillScore}</p>
            <p><strong>Độ khó kỹ thuật:</strong> ${difficultyScore}</p>
            <p><strong>Lỗi sai kỹ thuật:</strong> ${faultyCount}</p>
            <p><strong>Lỗi thừa thiếu động tác:</strong> ${surplusCount}</p>
            <p><strong>Lỗi thăng bằng/chạm đất:</strong> ${balanceCount}</p>
            <p><strong>Lỗi binh khí chạm đất:</strong> ${groundCount}</p>
            <hr>
            <h3 style="color: green;">Điểm cuối cùng: ${finalScore}</h3>
        </div>
    `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Xác Nhận',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            // Lưu vào localStorage
            localStorage.setItem(`weaponScore_${currentMatchId}_${blueParticipantId}`, JSON.stringify(performanceData));

            // Gửi dữ liệu tới server
            fetch(`${normalizedBaseUrlWeapon}ChamDiem/SubmitPerformanceScore`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(performanceData)
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Không thể gửi điểm');
                    }
                    return response.json();
                })
                .then(data => {
                    Swal.fire({
                        title: 'Gửi Điểm Thành Công',
                        text: 'Điểm số binh khí đã được lưu',
                        icon: 'success',
                        confirmButtonText: 'Đóng'
                    }).then(() => {
                        // Chuyển hướng sau khi lưu thành công
                        window.location.href = `${normalizedBaseUrlWeapon}Home/Index`;
                    });

                    // Xóa localStorage sau khi gửi thành công (tùy chọn)
                    localStorage.removeItem(`weaponScore_${currentMatchId}_${blueParticipantId}`);
                })
                .catch(error => {
                    console.error('Lỗi:', error);
                    Swal.fire({
                        title: 'Lỗi',
                        text: 'Không thể gửi điểm. Điểm số sẽ được lưu tạm thời.',
                        icon: 'warning',
                        confirmButtonText: 'Đóng'
                    });
                });
        }
    });
}

// Hàm kiểm tra và khôi phục điểm số từ localStorage
function checkLocalStorageScore() {
    const key = `weaponScore_${currentMatchId}_${blueParticipantId}`;
    const savedScore = localStorage.getItem(key);

    if (savedScore) {
        Swal.fire({
            title: 'Phát Hiện Điểm Số Chưa Gửi',
            text: 'Bạn có muốn khôi phục điểm số trước đó không?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Khôi Phục',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                const performanceData = JSON.parse(savedScore);

                // Khôi phục các điểm số
                document.getElementById('final-weapon-score').textContent = performanceData.finalScore.toFixed(2);

                // Hiển thị chi tiết điểm số
                Swal.fire({
                    title: 'Chi Tiết Điểm Số Đã Lưu',
                    html: performanceData.danhgia.replace(/\n/g, '<br>'),
                    icon: 'info',
                    confirmButtonText: 'Đóng'
                });
            } else {
                // Xóa localStorage nếu người dùng không muốn khôi phục
                localStorage.removeItem(key);
            }
        });
    }
}

// Gọi hàm kiểm tra khi trang được tải
window.addEventListener('DOMContentLoaded', () => {
    if (currentMatchId && blueParticipantId) {
        checkLocalStorageScore();
    }
});



function attachWeaponScoreEvents() {
    const weaponScoreIds = [
        { id: 'weapon-tech-score', max: 5, min: 0 },
        { id: 'weapon-skill-score', max: 3, min: 0 },
        { id: 'weapon-difficulty-score', max: 2, min: 0 }
    ];

    const weaponDeductIds = [
        { id: 'weapon-faulty-count', btn: 'weapon-faulty' },
        { id: 'weapon-surplus-count', btn: 'weapon-surplus' },
        { id: 'weapon-balance-count', btn: 'weapon-balance' },
        { id: 'weapon-ground-count', btn: 'weapon-ground' }
    ];

    // Sự kiện tăng/giảm điểm
    weaponScoreIds.forEach(item => {
        const incBtn = document.querySelector(`.score-inc[data-target='${item.id}']`);
        const decBtn = document.querySelector(`.score-dec[data-target='${item.id}']`);
        const scoreEl = document.getElementById(item.id);

        if (incBtn && scoreEl) {
            incBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.min(item.max, (val + 0.1));
                scoreEl.textContent = val.toFixed(1);
                updateWeaponFinalScore();
            });
        }

        if (decBtn && scoreEl) {
            decBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.max(item.min, (val - 0.1));
                scoreEl.textContent = val.toFixed(1);
                updateWeaponFinalScore();
            });
        }
    });

    // Sự kiện trừ điểm
    weaponDeductIds.forEach(item => {
        const btn = document.querySelector(`.deduction-btn[data-target='${item.btn}']`);
        const countEl = document.getElementById(item.id);

        if (btn && countEl) {
            btn.addEventListener('click', () => {
                let count = parseInt(countEl.textContent) || 0;
                count++;
                countEl.textContent = count;
                updateWeaponFinalScore();
            });
        }
    });
}

function updateWeaponFinalScore() {
    // Các tiêu chí điểm
    const weaponScoreIds = [
        { id: 'weapon-tech-score', max: 5 },
        { id: 'weapon-skill-score', max: 3 },
        { id: 'weapon-difficulty-score', max: 2 }
    ];

    // Các loại trừ điểm
    const weaponDeductIds = [
        { id: 'weapon-faulty-count', deduction: 0.2 },
        { id: 'weapon-surplus-count', deduction: 0.2 },
        { id: 'weapon-balance-count', deduction: 0.2 },
        { id: 'weapon-ground-count', deduction: 0.5 }
    ];

    // Tính tổng điểm từ các tiêu chí
    let total = 0;
    weaponScoreIds.forEach(item => {
        const el = document.getElementById(item.id);
        total += parseFloat(el?.textContent) || 0;
    });

    // Tính tổng điểm bị trừ
    let totalDeduct = 0;
    weaponDeductIds.forEach(item => {
        const count = parseInt(document.getElementById(item.id)?.textContent) || 0;
        totalDeduct += count * item.deduction;
    });

    // Tính điểm cuối cùng
    let final = total - totalDeduct;
    final = Math.max(0, Math.min(10, final)); // Giới hạn điểm từ 0 đến 10

    // Cập nhật điểm cuối cùng
    const finalScoreEl = document.getElementById('final-weapon-score');
    if (finalScoreEl) {
        finalScoreEl.textContent = final.toFixed(2);
    }
}