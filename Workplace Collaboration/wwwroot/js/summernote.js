$(document).ready(function () {
    $('.summernote').summernote({
        
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['fontname', ['fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['insert', ['link', 'picture', 'video']],
        ],

        maximumImageFileSize:5 * 1024 * 1024,
        maxHeight: 80,
        focus: true,

    });
});