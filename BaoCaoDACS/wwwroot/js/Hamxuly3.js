

//chamdiem bieudien
const baseUrlPerformance = (window.appBase || '/');
const normalizedBaseUrlPerformance = baseUrlPerformance.endsWith('/') ? baseUrlPerformance : `${baseUrlPerformance}/`;

function initPerformanceScoring() {

    // Gán sự kiện cho các nút
    attachPerformanceScoreEvents();

    // Khởi tạo điểm ban đầu
    updatePerfFinalScore();
    // Chỉ gán sự kiện submit, không gọi hàm ngay
    const submitPerformanceButton = document.getElementById('submit-performance');
    if (submitPerformanceButton) {
        submitPerformanceButton.addEventListener('click', handleSubmitPerformanceScore);
    }
}
function attachPerformanceScoreEvents() {
    // Danh sách các tiêu chí điểm
    const perfScoreIds = [
        { id: 'perf-tech-score', max: 5, min: 0, label: 'Kỹ thuật biểu diễn' },
        { id: 'perf-power-score', max: 3, min: 0, label: 'Sức mạnh/Nhịp điệu' },
        { id: 'perf-spirit-score', max: 2, min: 0, label: 'Thần thái' }
    ];

    // Danh sách các lỗi trừ điểm
    const perfDeductIds = [
        { id: 'perf-faulty-count', btn: 'perf-faulty', deduction: 0.2, label: 'Sai kỹ thuật' },
        { id: 'perf-surplus-count', btn: 'perf-surplus', deduction: 0.2, label: 'Thừa thiếu động tác' },
        { id: 'perf-balance-count', btn: 'perf-balance', deduction: 0.2, label: 'Mất thăng bằng' }
    ];

    // Sự kiện tăng/giảm điểm cho từng tiêu chí
    perfScoreIds.forEach(item => {
        const incBtn = document.querySelector(`.score-inc[data-target='${item.id}']`);
        const decBtn = document.querySelector(`.score-dec[data-target='${item.id}']`);
        const scoreEl = document.getElementById(item.id);

        if (incBtn && scoreEl) {
            incBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.min(item.max, (val + 0.1));
                scoreEl.textContent = val.toFixed(1);
                updatePerfFinalScore();
            });
        }

        if (decBtn && scoreEl) {
            decBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.max(item.min, (val - 0.1));
                scoreEl.textContent = val.toFixed(1);
                updatePerfFinalScore();
            });
        }
    });

    // Sự kiện trừ điểm cho các lỗi
    perfDeductIds.forEach(item => {
        const btn = document.querySelector(`.deduction-btn[data-target='${item.btn}']`);
        const countEl = document.getElementById(item.id);

        if (btn && countEl) {
            btn.addEventListener('click', () => {
                let count = parseInt(countEl.textContent) || 0;
                count++;
                countEl.textContent = count;
                updatePerfFinalScore();
            });
        }
    });

    // Sự kiện submit điểm

}

function updatePerfFinalScore() {
    // Danh sách các tiêu chí điểm (giữ nguyên như ở hàm trước)
    const perfScoreIds = [
        { id: 'perf-tech-score', max: 5, min: 0 },
        { id: 'perf-power-score', max: 3, min: 0 },
        { id: 'perf-spirit-score', max: 2, min: 0 }
    ];

    // Danh sách các lỗi trừ điểm (giữ nguyên như ở hàm trước)
    const perfDeductIds = [
        { id: 'perf-faulty-count', deduction: 0.2 },
        { id: 'perf-surplus-count', deduction: 0.2 },
        { id: 'perf-balance-count', deduction: 0.2 }
    ];

    // Tính tổng điểm từ các tiêu chí
    let total = 0;
    perfScoreIds.forEach(item => {
        const el = document.getElementById(item.id);
        total += parseFloat(el?.textContent) || 0;
    });

    // Tính tổng điểm bị trừ
    let totalDeduct = 0;
    perfDeductIds.forEach(item => {
        const count = parseInt(document.getElementById(item.id)?.textContent) || 0;
        totalDeduct += count * item.deduction;
    });

    // Tính điểm cuối cùng
    let final = total - totalDeduct;
    final = Math.max(0, Math.min(10, final)); // Giới hạn điểm từ 0 đến 10

    // Cập nhật điểm cuối cùng
    const finalScoreEl = document.getElementById('final-performance-score');
    if (finalScoreEl) {
        finalScoreEl.textContent = final.toFixed(2);
    }
}

window.handleSubmitPerformanceScore = async function () {


    // Lấy thông tin điểm số
    const finalScore = document.getElementById('final-performance-score')?.textContent || '0.0';
    const techScore = document.getElementById('perf-tech-score')?.textContent || '0.0';
    const powerScore = document.getElementById('perf-power-score')?.textContent || '0.0';
    const spiritScore = document.getElementById('perf-spirit-score')?.textContent || '0.0';

    // Lấy thông tin các lỗi
    const faultyCount = document.getElementById('perf-faulty-count')?.textContent || '0';
    const surplusCount = document.getElementById('perf-surplus-count')?.textContent || '0';
    const balanceCount = document.getElementById('perf-balance-count')?.textContent || '0';


        Swal.fire({
            title: 'Kết Quả Đánh Giá',
            html: `
        <div style="text-align: left; padding: 10px;">
            <p><strong>Kỹ thuật:</strong> ${techScore}</p>
            <p><strong>Sức mạnh/Nhịp điệu:</strong> ${powerScore}</p>
            <p><strong>Thần thái:</strong> ${spiritScore}</p>
            <p><strong>Lỗi sai kỹ thuật:</strong> ${faultyCount}</p>
            <p><strong>Lỗi thừa thiếu động tác:</strong> ${surplusCount}</p>
            <p><strong>Lỗi mất thăng bằng:</strong> ${balanceCount}</p>
            <hr>
            <h3 style="color: green;">Điểm cuối cùng: ${finalScore}</h3>
        </div>
    `,
            icon: 'info',
            confirmButtonText: 'Đóng',
            confirmButtonColor: '#3085d6'
        }).then(async (result) => {
            if (result.isConfirmed) {

                // Tạo đối tượng dữ liệu để gửi
                const performanceData = {
                    finalScore: parseFloat(finalScore),
                    matchId: currentMatchId,
                    ParticipantId: blueParticipantId,
                    danhgia: `Kết quả đánh giá biểu diễn:
                - Kỹ thuật: ${techScore}
                - Sức mạnh/Nhịp điệu: ${powerScore}
                - Thần thái: ${spiritScore}
                - Lỗi sai kỹ thuật: ${faultyCount}
                - Lỗi thừa thiếu động tác: ${surplusCount}
                - Lỗi mất thăng bằng: ${balanceCount}
                Điểm cuối cùng: ${finalScore}`
                };


                // Gửi dữ liệu tới server
                const response = await fetch(`${normalizedBaseUrlPerformance}ChamDiem/SubmitPerformanceScore`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(performanceData)
                }).then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(errorData.message || 'Lỗi khi submit kết quả');
                        });
                    }
                    return response.json();
                })
                    .then(data => {
                        Swal.fire({
                            icon: 'success',
                            title: 'Cập Nhật Thành Công',
                            html: `
                            <p>Đã lưu kết quả trận đấu</p>
                        `,
                            confirmButtonText: 'OK'
                        }).then(() => {
                            window.location.href = `${normalizedBaseUrlPerformance}Home/Index`;
                        });
                    })
                    .catch(error => {
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
