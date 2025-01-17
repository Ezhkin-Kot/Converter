const url = 'https://deciding-logically-piglet.ngrok-free.app/sessions';
const user = JSON.parse(sessionStorage.getItem("user"));

document.getElementById('avatar').innerText = user.login.charAt(0);
document.getElementById('username').innerText = user.login;

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('uploadForm');
    const fileInput = document.getElementById('fileInput');
    const downloadLinksContainer = document.getElementById('downloadLinks');

    // Ensure required elements exist
    if (!form || !fileInput || !downloadLinksContainer) {
        console.error('Required elements are missing in the DOM.');
        return;
    }

    // Handle form submission
    form.addEventListener('submit', (event) => {
        event.preventDefault();

        const files = fileInput.files;

        if (!files.length) {
            alert('Please select at least one file.');
            return;
        }

        // Clear previous download links
        downloadLinksContainer.innerHTML = '';

        Array.from(files).forEach((file) => {
            // Validate file type based on conversion type
            if (
                (conversionType === 'jpg-to-png' && !file.type.startsWith('image/jpeg')) ||
                (conversionType === 'png-to-jpg' && !file.type.startsWith('image/png'))
            ) {
                alert(`File ${file.name} is not a valid ${conversionType === 'jpg-to-png' ? 'JPG' : 'PNG'} file.`);
                return;
            }

            const reader = new FileReader();

            reader.onload = (e) => {
                const img = new Image();
                img.onload = () => {
                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    canvas.width = img.width;
                    canvas.height = img.height;

                    ctx.drawImage(img, 0, 0);

                    // Determine output format
                    let outputDataUrl, outputFilename;
                    if (conversionType === 'jpg-to-png') {
                        outputDataUrl = canvas.toDataURL('image/png');
                        outputFilename = `${file.name.split('.')[0]}-converted.png`;
                    } else if (conversionType === 'png-to-jpg') {
                        outputDataUrl = canvas.toDataURL('image/jpeg', 0.9); // Use 90% quality for JPG
                        outputFilename = `${file.name.split('.')[0]}-converted.jpg`;
                    }

                    // Create and display download link
                    const link = document.createElement('a');
                    link.href = outputDataUrl;
                    link.download = outputFilename;
                    link.textContent = `Download ${outputFilename}`;
                    link.style.display = 'block';

                    downloadLinksContainer.appendChild(link);
                };

                img.src = e.target.result;
            };

            reader.onerror = () => {
                alert(`Failed to read file: ${file.name}`);
            };

            let json;
            try {
                fetch(`${url}/upd/${user.id}`, {
                    method: 'PUT',
                }).then((response) => response.json()).then((data) => {
                    console.log(data);

                    if (data.value.amount > 0) {
                        reader.readAsDataURL(file);
                    } else {
                        alert('The number of free conversions has expired');
                    }
                })
            }
            catch (error) {
                console.log(error);
            }
        });
    });
});

async function LogOut() {
    try {
        const response = await fetch(`${url}/close/${user.id}`, {
            method: 'PUT',
        });
        json = await response.json();
        console.log(json);

        window.location.href = 'authentication.html';
    }
    catch (error) {
        console.log(error);
    }
}