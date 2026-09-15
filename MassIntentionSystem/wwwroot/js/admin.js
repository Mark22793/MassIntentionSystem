document.addEventListener('DOMContentLoaded', function () {

    // 1. Sidebar Toggle Functionality
    const sidebarToggle = document.getElementById('sidebarToggle');
    const wrapper = document.getElementById('wrapper');

    if (sidebarToggle && wrapper) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            wrapper.classList.toggle('toggled');
        });
    }

    // 2. View Proof of Payment Receipt Modal Trigger
    const receiptModal = document.getElementById('receiptModal');
    const modalReceiptImg = document.getElementById('modalReceiptImg');

    if (receiptModal && modalReceiptImg) {
        document.querySelectorAll('.btn-view-receipt').forEach(button => {
            button.addEventListener('click', function () {
                const imgPath = this.getAttribute('data-receipt-path');
                const refNo = this.getAttribute('data-ref-no');

                modalReceiptImg.src = imgPath ? imgPath : '/images/no-receipt.png';
                const modalTitle = document.getElementById('receiptModalLabel');
                if (modalTitle) modalTitle.innerText = 'Proof of Payment - Ref: ' + refNo;
            });
        });
    }

    // 3. Quick Table Search Filter (Filter by Reference Code or Name)
    const adminSearchInput = document.getElementById('adminSearchInput');
    const intentionsTableBody = document.getElementById('intentionsTableBody');

    if (adminSearchInput && intentionsTableBody) {
        adminSearchInput.addEventListener('keyup', function () {
            const filterValue = this.value.toLowerCase();
            const rows = intentionsTableBody.getElementsByTagName('tr');

            Array.from(rows).forEach(row => {
                const textContent = row.textContent.toLowerCase();
                if (textContent.indexOf(filterValue) > -1) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }

    // 4. Export Document Validation Trigger
    const docExportForm = document.getElementById('docExportForm');
    if (docExportForm) {
        docExportForm.addEventListener('submit', function (e) {
            const priestSelect = document.getElementById('priestSelect');
            if (priestSelect && priestSelect.value === '') {
                e.preventDefault();
                alert('Paki-pili ang Paring magmisa (Celebrant) bago i-generate ang Word Document.');
            }
        });
    }
});

// 5. AJAX Payment Status Verification (Approved/Rejected)
function verifyPayment(intentionId, status) {
    if (!confirm(`Sigurado ka bang gusto mong baguhin ang status bilang "${status}"?`)) {
        return;
    }

    fetch('/Payment/VerifyStatus', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({
            id: intentionId,
            status: status
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Matagumpay na na-update ang payment status!');
                location.reload(); // Refresh ang dashboard table
            } else {
                alert('Nagkaroon ng problema: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Nagkaroon ng error sa pag-process ng request.');
        });
}