const url = window.location.hostname === 'localhost'
    ? 'http://localhost:8080/users/prem'
    : '/api/users/prem';
const user = JSON.parse(sessionStorage.getItem("user"));

async function SetFreePlan() {
    const boxes = document.querySelectorAll('.box');
    boxes.forEach((box) => {box.style.opacity = '0';});

    console.log(user);

    try {
        const response = await fetch(`${url}/${user.id}/${false}`, {
            method: 'PUT',
        });
        const json = await response.json();
        console.log(json.message);

        if (response.ok) {
            window.location.href = 'jpg-png.html';
        }
    } catch (error) {
        console.log(error);
    }
}

async function SetPremPlan() {
    const boxes = document.querySelectorAll('.box');
    boxes.forEach((box) => {box.style.opacity = '0';});

    console.log(user);

    try {
        const response = await fetch(`${url}/${user.id}/${true}`, {
            method: 'PUT',
        });
        const json = await response.json();
        console.log(json.message);

        if (response.ok) {
            window.location.href = 'jpg-png.html';
        }
    } catch (error) {
        console.log(error);
    }
}