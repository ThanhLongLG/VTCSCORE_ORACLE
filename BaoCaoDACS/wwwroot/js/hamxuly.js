
//chấm điểm Biễu diền
const baseUrlTeam = (window.appBase || '/');
const normalizedBaseUrlTeam = baseUrlTeam.endsWith('/') ? baseUrlTeam : `${baseUrlTeam}/`;

window.initTeamScoring = function () {
    attachTeamScoringEvents();
    updateTeamScoringEvents();
    // Sự kiện submit
    const submitWeaponButton = document.getElementById('submit-weapon');
    if (submitWeaponButton) {
        submitWeaponButton.addEventListener('click', handleSubmitTeamScore);
    }
}



// ham bieudien
function attachTeamScoringEvents() {
    // Danh sách các tiêu chí điểm
    const teamScoreIds = [
        { id: 'team-tech-score', max: 4, min: 0, label: 'Kỹ thuật biểu diễn' },
        { id: 'team-power-score', max: 2, min: 0, label: 'Phối hợp/Nhịp điệu' },
        { id: 'team-spirit-score', max: 2, min: 0, label: 'Thẩm mỹ/Thần thái' },
        { id: 'team-creativity-score', max: 1, min: 0, label: 'Điểm sáng tạo' },
        { id: 'team-time-score', max: 1, min: 0, label: 'Thời gian biểu diễn' }
    ];

    // Danh sách các lỗi trừ điểm
    const teamDeductIds = [
        { id: 'team-faulty-count', btn: 'team-faulty', deduction: 0.2, label: 'Sai kỹ thuật' },
        { id: 'team-rhythm-count', btn: 'team-rhythm', deduction: 0.2, label: 'Sai nhịp điệu' },
        { id: 'team-overtime-count', btn: 'team-overtime', deduction: 0.2, label: 'Lố thời gian' },
        { id: 'team-costume-count', btn: 'team-costume', deduction: 0.2, label: 'Lỗi trang phục' }
    ];

    // Sự kiện tăng/giảm điểm cho từng tiêu chí
    teamScoreIds.forEach(item => {
        const incBtn = document.querySelector(`.score-inc[data-target='${item.id}']`);
        const decBtn = document.querySelector(`.score-dec[data-target='${item.id}']`);
        const scoreEl = document.getElementById(item.id);

        if (incBtn && scoreEl) {
            incBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.min(item.max, (val + 0.1));
                scoreEl.textContent = val.toFixed(1);
                updateTeamFinalScore();
            });
        }

        if (decBtn && scoreEl) {
            decBtn.addEventListener('click', () => {
                let val = parseFloat(scoreEl.textContent) || 0;
                val = Math.max(item.min, (val - 0.1));
                scoreEl.textContent = val.toFixed(1);
                updateTeamFinalScore();
            });
        }
    });

    // Sự kiện trừ điểm cho các lỗi
    teamDeductIds.forEach(item => {
        const btn = document.querySelector(`.deduction-btn[data-target='${item.btn}']`);
        const countEl = document.getElementById(item.id);

        if (btn && countEl) {
            btn.addEventListener('click', () => {
                let count = parseInt(countEl.textContent) || 0;
                count++;
                countEl.textContent = count;
                updateTeamFinalScore();
            });
        }
    });

    // Sự kiện nút loại trực tiếp
    const teamDirectBtn = document.getElementById('team-direct-btn');
    const teamDirectStatus = document.getElementById('team-direct-status');
    let teamDirectEliminated = false;

    if (teamDirectBtn) {
        teamDirectBtn.addEventListener('click', () => {
            if (!teamDirectEliminated) {
                if (confirm('Xác nhận loại trực tiếp đội này?')) {
                    teamDirectStatus.textContent = 'ĐÃ LOẠI TRỰC TIẾP';
                    teamDirectEliminated = true;
                    teamDirectBtn.disabled = true;

                    // Khóa các nút chấm điểm khác
                    document.querySelectorAll('#team-scoring .score-inc, #team-scoring .score-dec, #team-scoring .deduction-btn').forEach(btn => {
                        if (btn !== teamDirectBtn) btn.disabled = true;
                    });

                    // Cập nhật điểm cuối
                    updateTeamFinalScore();
                }
            }
        });
    }

    // Sự kiện submit điểm
    const submitTeamButton = document.getElementById('submit-team');
    if (submitTeamButton) {
        submitTeamButton.addEventListener('click', handleSubmitTeamScore);
    }
}


async function handleSubmitTeamScore() {
    try {
        // Lấy thông tin điểm số
        const finalScore = document.getElementById('final-team-score')?.textContent || '0.0';
        const techScore = document.getElementById('team-tech-score').textContent || '0.0';
        const powerScore = document.getElementById('team-power-score').textContent || '0.0';
        const spiritScore = document.getElementById('team-spirit-score').textContent || '0.0';
        const creativityScore = document.getElementById('team-creativity-score').textContent || '0.0';
        const timeScore = document.getElementById('team-time-score').textContent || '0.0';

        // Lấy thông tin các lỗi
        const faultyCount = document.getElementById('team-faulty-count')?.textContent || '0';
        const rhythmCount = document.getElementById('team-rhythm-count')?.textContent || '0';
        const overtimeCount = document.getElementById('team-overtime-count')?.textContent || '0';
        const costumeCount = document.getElementById('team-costume-count')?.textContent || '0';
        const directStatus = document.getElementById('team-direct-status')?.textContent || '';

        // Lấy thông tin đội thi
        const teamName = document.getElementById('team-name')?.textContent || 'Chưa xác định';
        const teamCode = document.getElementById('team-code')?.textContent || '';


        // Tạo đối tượng dữ liệu để gửi
        const teamData = {
            finalScore: parseFloat(finalScore),
            matchId: currentMatchId,
            ParticipantId: blueParticipantId,
            loaitructiep: directStatus ? "1" : null,
            danhgia: `Kết quả đánh giá đội:
            - Kỹ thuật: ${techScore}
            - Phối hợp/Nhịp điệu: ${powerScore}
            - Thẩm mỹ/Thần thái: ${spiritScore}
            - Điểm sáng tạo: ${creativityScore}
            - Thời gian biểu diễn: ${timeScore}
            Các lỗi:
            - Lỗi sai kỹ thuật: ${faultyCount}
            - Lỗi sai nhịp điệu: ${rhythmCount}
            - Lỗi lố thời gian: ${overtimeCount}
            - Lỗi trang phục: ${costumeCount}
            Trạng thái loại trực tiếp: ${directStatus || 'Không'}`
        };

        // Hiển thị xác nhận trước khi submit
        const confirmSubmit = await Swal.fire({
            title: 'Xác Nhận Gửi Điểm Đồng đội',
            html: `
        <div style="text-align: left; padding: 10px;">
            <p><strong>Kỹ thuật biểu diễn:</strong> ${techScore}</p>
            <p><strong>Phối hợp/Nhịp điệu:</strong> ${powerScore}</p>
            <p><strong>Thẩm mỹ/Thần thái:</strong> ${spiritScore}</p>
            <p><strong>Điểm sáng tạo:</strong> ${creativityScore}</p>
            <p><strong>Thời gian biểu diễn:</strong> ${timeScore}</p>
            <p><strong>Sai kỹ thuật:</strong> ${faultyCount}</p>
            <p><strong>Lỗi thời gian:</strong> ${overtimeCount}</p>
            <p><strong>Sai nhịp điệu:</strong> ${rhythmCount}</p>
             <p><strong>Lỗi trang phục:</strong> ${costumeCount}</p>
              <p><strong>Trạng thái loại trực tiếp:</strong> ${directStatus || 'Không'}</p>
            <hr>
            <h3 style="color: green;">Điểm cuối cùng: ${finalScore}</h3>
        </div>
    `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Xác Nhận',
            cancelButtonText: 'Hủy'
        });

        if (confirmSubmit.isConfirmed) {
            // Gửi dữ liệu tới server
            const response = await fetch(`${normalizedBaseUrlTeam}ChamDiem/SubmitPerformanceScore`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(teamData)
            });

            // Kiểm tra phản hồi từ server
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Lỗi khi submit điểm');
            }

            const result = await response.json();

            // Hiển thị thông báo thành công
            await Swal.fire({
                icon: 'success',
                title: 'Lưu Điểm Thành Công',
                html: `
                    <p>Đã lưu kết quả cho đội ${teamName}</p>
                    <p>Trạng Thái: ${result.matchStatus || 'Đã cập nhật'}</p>
                `,
                confirmButtonText: 'OK'
            });

            // Chuyển hướng hoặc làm mới trang
            window.location.href = `${normalizedBaseUrlTeam}Home/Index`;
        }
    } catch (error) {
        // Xử lý lỗi
        console.error('Lỗi khi submit điểm:', error);
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: error.message || 'Không thể lưu điểm. Vui lòng thử lại.',
            confirmButtonText: 'Đóng'
        });
    }
}
function updateTeamFinalScore() {
    // Danh sách các tiêu chí điểm
    const teamScoreIds = [
        { id: 'team-tech-score', max: 4, min: 0, weight: 1, label: 'Kỹ thuật biểu diễn' },
        { id: 'team-power-score', max: 2, min: 0, weight: 1, label: 'Phối hợp/Nhịp điệu' },
        { id: 'team-spirit-score', max: 2, min: 0, weight: 1, label: 'Thẩm mỹ/Thần thái' },
        { id: 'team-creativity-score', max: 1, min: 0, weight: 1, label: 'Điểm sáng tạo' },
        { id: 'team-time-score', max: 1, min: 0, weight: 1, label: 'Thời gian biểu diễn' }
    ];

    // Danh sách các lỗi trừ điểm
    const teamDeductIds = [
        { id: 'team-faulty-count', btn: 'team-faulty', deduction: 0.2, label: 'Sai kỹ thuật' },
        { id: 'team-rhythm-count', btn: 'team-rhythm', deduction: 0.2, label: 'Sai nhịp điệu' },
        { id: 'team-overtime-count', btn: 'team-overtime', deduction: 0.2, label: 'Lố thời gian' },
        { id: 'team-costume-count', btn: 'team-costume', deduction: 0.2, label: 'Lỗi trang phục' }
    ];

    // Kiểm tra trạng thái loại trực tiếp
    const directEliminationEl = document.getElementById('team-direct-status');
    const isDirectEliminated = directEliminationEl && directEliminationEl.textContent.trim() !== '';

    // Nếu bị loại trực tiếp, điểm = 0
    if (isDirectEliminated) {
        document.getElementById('final-team-score').textContent = '0.00';
        return;
    }

    // Tính tổng điểm từ các tiêu chí
    let total = 0;
    let maxPossibleScore = 0;

    teamScoreIds.forEach(item => {
        const el = document.getElementById(item.id);
        const score = parseFloat(el?.textContent) || 0;
        const weightedScore = score * item.weight;

        total += weightedScore;
        maxPossibleScore += item.max * item.weight;
    });

    // Tính tổng điểm bị trừ
    let totalDeduct = 0;
    teamDeductIds.forEach(item => {
        const count = parseInt(document.getElementById(item.id)?.textContent) || 0;
        totalDeduct += count * item.deduction;
    });

    // Tính điểm cuối cùng
    let final = total - totalDeduct;

    // Giới hạn điểm từ 0 đến 10
    final = Math.max(0, Math.min(10, final));

    // Hiển thị điểm
    const finalScoreEl = document.getElementById('final-team-score');
    if (finalScoreEl) {
        finalScoreEl.textContent = final.toFixed(2);
    }

    // Tùy chọn: Hiển thị chi tiết điểm (nếu cần)
    console.log('Scoring Breakdown:', {
        totalScore: total.toFixed(2),
        maxPossibleScore: maxPossibleScore.toFixed(2),
        deductions: totalDeduct.toFixed(2),
        finalScore: final.toFixed(2)
    });

    // Thêm màu sắc để dễ nhận biết mức điểm
    updateScoreColor(final);
}

// Hàm hỗ trợ: Thay đổi màu điểm dựa trên giá trị
function updateScoreColor(score) {
    const finalScoreEl = document.getElementById('final-team-score');
    if (!finalScoreEl) return;

    if (score >= 8) {
        finalScoreEl.style.color = 'green';
    } else if (score >= 6) {
        finalScoreEl.style.color = 'orange';
    } else if (score >= 4) {
        finalScoreEl.style.color = 'darkorange';
    } else {
        finalScoreEl.style.color = 'red';
    }
}