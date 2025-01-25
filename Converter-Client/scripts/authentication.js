const url = window.location.hostname === 'localhost'
    ? 'http://localhost:8080/sessions/auth'
    : '/api/sessions/auth';

document.getElementById('auth-form').addEventListener('submit', async function (event) {
    event.preventDefault();

    let login = document.getElementById('login').value;
    let pass = document.getElementById('password').value;
    let successMessage = document.getElementById('success-message');
    let errorMessage = document.getElementById('error-message');
    let json;

    let data = {
        login: login,
        password: pass,
    }

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            json = await response.json();
            console.log(json);

            // Saving user data
            sessionStorage.setItem("user", JSON.stringify({
                id: json.value.user.id,
                login: json.value.user.login,
                premium: json.value.user.premium,
            }));
            console.log(sessionStorage.getItem("user"));

            successMessage.style.color = 'green';
            successMessage.style.opacity = '1';
            setTimeout(() => {
                    window.location.href = 'jpg-png.html';
                }, 1000);
        } else if (response.status === 401) {
            const errorJson = await response.json();
            console.error(errorJson.message);

            errorMessage.style.opacity = '1';
            setTimeout(() => {
                errorMessage.style.opacity = '0';
                }, 2000);
        } else if (response.status === 400) {
            const errorJson = await response.json();
            console.error('Bad request:', errorJson.message);
        } else {
            console.error('Unexpected response:', response.status, await response.json());
        }
    } catch (error) {
        console.log('Authentication request error:', error);
    }
});