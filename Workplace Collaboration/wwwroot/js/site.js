// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function validateAndSubmitForm(event) {
    event.preventDefault();

    var summernoteElement = document.querySelector('.summernote');
    var content = summernoteElement.value;

    var modContent = content.replace(/\&nbsp;/g, '');  // remove non-breaking spaces

    modContent = modContent.replace(/^\s+|\s+$/g, ''); //remove spaces

    // if modContent is not empty i.e.the string contains characters other than spaces
    if (modContent !== "") {
        document.getElementById('messageForm').submit();
    } else {
        return false;
    }
}  