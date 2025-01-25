const url = window.location.hostname === 'localhost'
    ? 'http://localhost:8080'
    : '/api';

document.getElementById('reg-form').addEventListener('submit', async function (event) {
    event.preventDefault();

    let login = document.getElementById('login').value;
    let pass = document.getElementById('password').value;
    let passConfirm = document.getElementById('pass-confirm').value;
    let successMessage = document.getElementById('success-message');
    let existsMessage = document.getElementById('user-exists-message');
    let errorMessage = document.getElementById('error-message');
    let data;
    let json;

    if (pass !== passConfirm) {
        errorMessage.style.opacity = '1';
        setTimeout(() => {
            errorMessage.style.opacity = '0';
            }, 2000);
    } else {
        data = {
            login: login,
            password: pass,
        }

        try {
            const response = await fetch(`${url}/users/reg`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data),
            });

            if (response.ok) { // Status-code checking
                json = await response.json();
                console.log(json);

                try {
                    const loginResponse = await fetch(`${url}/sessions/auth`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(data),
                    });

                    if (loginResponse.ok) {
                        const loginJson = await loginResponse.json();
                        console.log(loginJson);

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
                            window.location.href = 'plan.html';
                            }, 1000);
                    } else {
                        console.error('Login failed:', await loginResponse.json());
                    }
                } catch (error) {
                    console.error('Login request error:', error);
                }
            } else if (response.status === 409) {
                const errorJson = await response.json();
                console.error('User already exists:', errorJson.message);

                existsMessage.style.opacity = '1';
                setTimeout(() => {
                    existsMessage.style.opacity = '0';
                    }, 2000);
            } else if (response.status === 400) {
                const errorJson = await response.json();
                console.error('Bad request:', errorJson.message);
            } else {
                console.error('Unexpected response:', response.status, await response.json());
            }
        } catch (error) {
            console.error('Register request error:', error);
        }
    }
});