document.addEventListener('DOMContentLoaded', function () {

    // 1. Dynamic Category Guidance & Textarea Placeholder Update
    const categorySelect = document.getElementById('categorySelect');
    const offeringNamesInput = document.getElementById('offeringNamesInput');
    const categoryHelpText = document.getElementById('categoryHelpText');

    if (categorySelect && offeringNamesInput) {
        categorySelect.addEventListener('change', function () {
            const selectedVal = this.value;

            switch (selectedVal) {
                case '1': // Healing
                    offeringNamesInput.placeholder = "Halimbawa: Juan Dela Cruz, Maria Santos (Maitatala para sa kagalingan at kalusugan)";
                    if (categoryHelpText) categoryHelpText.innerText = "Ilista ang mga pangalan ng may-sakit na ipagdaralibang magaling.";
                    break;
                case '2': // Thanksgiving
                    offeringNamesInput.placeholder = "Halimbawa: Dela Cruz Family, Birthday of Juan, Passers of Board Exam";
                    if (categoryHelpText) categoryHelpText.innerText = "Ilista ang mga pangalan o pamilya na nagpapasalamat sa natanggap na biyaya.";
                    break;
                case '3': // Eternal Repose
                    offeringNamesInput.placeholder = "Halimbawa: + Juan Dela Cruz, + Maria Santos, + All Souls in Purgatory";
                    if (categoryHelpText) categoryHelpText.innerText = "Lagyan ng '+' sa unahan ng pangalan ng mga sumakabilang-buhay.";
                    break;
                case '4': // Other Intentions
                    offeringNamesInput.placeholder = "Halimbawa: Special Intention for Family Peace, Safe Travel of Pedro";
                    if (categoryHelpText) categoryHelpText.innerText = "Ilista ang iba pang espesyal na kahilingan at petisyon.";
                    break;
                default:
                    offeringNamesInput.placeholder = "I-type dito ang mga pangalan ng ipagdarasal...";
                    if (categoryHelpText) categoryHelpText.innerText = "";
            }
        });
    }

    // 2. Proof of Payment Image Preview (Public Upload)
    const receiptUploadInput = document.getElementById('receiptUploadInput');
    const receiptPreviewImg = document.getElementById('receiptPreviewImg');
    const previewContainer = document.getElementById('previewContainer');

    if (receiptUploadInput && receiptPreviewImg) {
        receiptUploadInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                // Validation: Dapat Image file lang
                if (!file.type.match('image.*')) {
                    alert('Paki-upload lamang ang valid na larawan (JPG, PNG, o JPEG).');
                    this.value = '';
                    if (previewContainer) previewContainer.classList.add('d-none');
                    return;
                }

                const reader = new FileReader();
                reader.onload = function (e) {
                    receiptPreviewImg.src = e.target.result;
                    if (previewContainer) previewContainer.classList.remove('d-none');
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // 3. Form Submission Client Validation (Date Check)
    const intentionForm = document.getElementById('massIntentionForm');
    const massDateInput = document.getElementById('massDateInput');

    if (intentionForm && massDateInput) {
        intentionForm.addEventListener('submit', function (e) {
            const selectedDate = new Date(massDateInput.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                e.preventDefault();
                alert('Hindi pwedeng pumili ng nakalipas na petsa para sa Misa.');
            }
        });
    }
});