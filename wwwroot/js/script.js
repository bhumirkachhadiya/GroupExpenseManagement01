// Get elements
const openPopup = document.getElementById('openPopup');
const closePopup = document.getElementById('closePopup');
const popup = document.getElementById('popup');

// Open the popup
openPopup.addEventListener('click', () => {
  popup.style.display = 'block';
});

// Close the popup
closePopup.addEventListener('click', () => {
  popup.style.display = 'none';
});

// Close the popup when clicking outside of it
window.addEventListener('click', (event) => {
  if (event.target === popup) {
    popup.style.display = 'none';
  }
});


// Get elements for photo popup
const openPhotoPopup = document.getElementById('openPhotoPopup');
const closePhotoPopup = document.getElementById('closePhotoPopup');
const photoPopup = document.getElementById('photoPopup');

// Open the photo popup
openPhotoPopup.addEventListener('click', () => {
    photoPopup.style.display = 'block';
});

// Close the photo popup
closePhotoPopup.addEventListener('click', () => {
    photoPopup.style.display = 'none';
});

// Close the photo popup when clicking outside of it
window.addEventListener('click', (event) => {
    if (event.target === photoPopup) {
        photoPopup.style.display = 'none';
    }
});

//// Get elements for forgotten password popup
//const openForgottenPopup = document.getElementById('openForgottenPopup');
//const closeForgottenPopup = document.getElementById('closeForgottenPopup');
//const forgottenPopup = document.getElementById('forgottenPopup');

//// Open the forgotten password popup
//openForgottenPopup.addEventListener('click', () => {
//    forgottenPopup.style.display = 'block';
//});

//// Close the forgotten password popup
//closeForgottenPopup.addEventListener('click', () => {
//    forgottenPopup.style.display = 'none';
//});

//// Close the forgotten password popup when clicking outside of it
//window.addEventListener('click', (event) => {
//    if (event.target === forgottenPopup) {
//        forgottenPopup.style.display = 'none';
//    }
//});


function triggerFileInput() {
    document.getElementById('imageUpload').click(); // Simulate click on file input
}

function previewImage(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            console.log("Preview Image URL: ", e.target.result); // Display image URL (optional)
            // Add code to display the preview if needed
        };
        reader.readAsDataURL(file);
    }
}

// Function to handle image preview when a file is selected
function previewImage(event) {
    var imagePreview = document.getElementById('imagePreview');
    var removeButton = document.getElementById('removeImageButton');
    var file = event.target.files[0];

    if (file) {
        var reader = new FileReader();

        reader.onload = function (e) {
            imagePreview.src = e.target.result;
            imagePreview.style.display = 'block'; // Show the image once it's loaded
            removeButton.style.display = 'block'; // Show the remove button
        }

        reader.readAsDataURL(file); // Convert the image file to a data URL
    } else {
        imagePreview.src = '#';
        imagePreview.style.display = 'none'; // Hide the image if no file is selected
        removeButton.style.display = 'none'; // Hide the remove button
    }
}

// Function to handle removing the selected image
function removeImage() {
    var imageUpload = document.getElementById('imageUpload');
    var imagePreview = document.getElementById('imagePreview');
    var removeButton = document.getElementById('removeImageButton');

    // Reset the file input and hide the image preview
    imageUpload.value = '';
    imagePreview.src = '#';
    imagePreview.style.display = 'none';
    removeButton.style.display = 'none';

    // Set its value to null Of photoPath
    var photoPathInput = document.getElementById("photoPath");
    photoPathInput.value = null; // This will set the value to the string "null"
}