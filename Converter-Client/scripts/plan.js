const url = 'https://deciding-logically-piglet.ngrok-free.app/users/prem';
const user = JSON.parse(sessionStorage.getItem("user"));

async function SetFreePlan() {
    const boxes = document.querySelectorAll('.box');
    boxes.forEach((box) => {box.style.opacity = '0';});

    console.log(user);
    let json;

    try {
        const response = await fetch(`${url}/${user.id}/${false}`, {
            method: 'PUT',
        });
        json = await response.json();
        console.log(json);

        if (json.value.success) {
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
    let json;

    try {
        const response = await fetch(`${url}/${user.id}/${true}`, {
            method: 'PUT',
        });
        json = await response.json();
        console.log(json);

        if (json.value.success) {
            window.location.href = 'jpg-png.html';
        }
    } catch (error) {
        console.log(error);
    }
}