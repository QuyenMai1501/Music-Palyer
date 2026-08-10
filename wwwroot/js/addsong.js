  // Tùy chỉnh thông báo lỗi của jQuery Validation bằng tiếng Việt
    jQuery.extend(jQuery.validator.messages, {
        required: "Trường này không được để trống."
    });

    document.addEventListener('DOMContentLoaded', function () {
        console.log('Script đã tải'); // Ghi log để kiểm tra script chạy

        // Lấy các input
        const uploadImageInput = document.querySelector('#uploadImage');
        const titleInput = document.querySelector('#title');
        const artistInput = document.querySelector('#artist');
        const uploadFileInput = document.querySelector('#uploadFile');
        const uploadImageError = uploadImageInput.nextElementSibling;
        const titleError = titleInput.nextElementSibling;
        const artistError = artistInput.nextElementSibling;
        const uploadFileError = uploadFileInput.nextElementSibling;

        // Kiểm tra ảnh bìa (tùy chọn)
        function validateUploadImage() {
            const allowedExtensions = ['jpg', 'jpeg', 'png'];
            const file = uploadImageInput.files[0];
            if (file) {
                const extension = file.name.split('.').pop().toLowerCase();
                if (!allowedExtensions.includes(extension)) {
                    uploadImageInput.classList.add('invalid');
                    uploadImageError.textContent = 'File ảnh phải có định dạng .jpg, .jpeg hoặc .png.';
                } else {
                    uploadImageInput.classList.remove('invalid');
                    uploadImageError.textContent = '';
                }
            } else {
                uploadImageInput.classList.remove('invalid');
                uploadImageError.textContent = '';
            }
        }

        // Kiểm tra tiêu đề
        function validateTitle() {
            if (!titleInput.value.trim()) {
                titleInput.classList.add('invalid');
                titleError.textContent = 'Tiêu đề bài hát không được để trống.';
            } else {
                titleInput.classList.remove('invalid');
                titleError.textContent = '';
            }
        }

        // Kiểm tra nghệ sĩ
        function validateArtist() {
            if (!artistInput.value.trim()) {
                artistInput.classList.add('invalid');
                artistError.textContent = 'Tên nghệ sĩ không được để trống.';
            } else {
                artistInput.classList.remove('invalid');
                artistError.textContent = '';
            }
        }

        // Kiểm tra file nhạc
        function validateUploadFile() {
            const allowedExtension = 'mp3';
            const file = uploadFileInput.files[0];
            if (!file) {
                uploadFileInput.classList.add('invalid');
                uploadFileError.textContent = 'Vui lòng chọn file nhạc.';
            } else {
                const extension = file.name.split('.').pop().toLowerCase();
                if (extension !== allowedExtension) {
                    uploadFileInput.classList.add('invalid');
                    uploadFileError.textContent = 'File nhạc phải có định dạng .mp3.';
                } else {
                    uploadFileInput.classList.remove('invalid');
                    uploadFileError.textContent = '';
                }
            }
        }

        // Gắn sự kiện input
        uploadImageInput.addEventListener('change', validateUploadImage);
        titleInput.addEventListener('input', validateTitle);
        artistInput.addEventListener('input', validateArtist);
        uploadFileInput.addEventListener('change', validateUploadFile);

        // Tùy chỉnh jQuery Validation
        $("#addSongForm").validate({
            errorClass: "error-message",
            errorElement: "span",
            errorPlacement: function (error, element) {
                // Đặt thông báo lỗi dưới input
                error.insertAfter(element);
            },
            rules: {
                Title: {
                    required: true
                },
                Artist: {
                    required: true
                },
                UploadFile: {
                    required: true
                }
            },
            messages: {
                Title: {
                    required: "Tiêu đề bài hát không được để trống."
                },
                Artist: {
                    required: "Tên nghệ sĩ không được để trống."
                },
                UploadFile: {
                    required: "Vui lòng chọn file nhạc."
                }
            }
        });

        // Kiểm tra khi submit form
        const form = document.querySelector('#addSongForm');
        form.addEventListener('submit', function (e) {
            console.log('Đã cố gắng submit form'); // Debug
            validateUploadImage();
            validateTitle();
            validateArtist();
            validateUploadFile();

            // Ngăn submit nếu có lỗi
            if (titleInput.classList.contains('invalid') ||
                artistInput.classList.contains('invalid') ||
                uploadFileInput.classList.contains('invalid')) {
                e.preventDefault();
            }
        });
    });
    document.addEventListener('DOMContentLoaded', function () {
    const successMessage = document.getElementById('successMessage');
        if (successMessage) {
            // Ẩn thông báo với hiệu ứng mờ dần sau 5 giây
            setTimeout(function () {
                $(successMessage).fadeOut(500); // Mờ dần trong 500ms
            }, 5000);
        }
    });