function ConstructWS(url) {
    Process.ws = new WebSocket(url);
}

function ServerResponse(action) {

    return new Promise((resolve, reject) => {

        Process.ws.onmessage = function (event) {

            let response = JSON.parse(event.data);

            if (response[0] == null) return;

            if (response[0] == 'Progress') {
                Home.progress.update(response[1], response[2]);
                return;
            }

            if (response[0] == 'Exception') {
                alert(response[1]);
                return;
            }

            if (response[0] != action) {
                alert('Action ' + action + ' not found in server response');
                return;
            }

            resolve(response[1]);
        }
    });
}

function setAttributes(element, properties) {
    Object.entries(properties).forEach(([key, value]) => element.setAttribute(key, value));
}

function removeAttributes(element, properties) {
    properties.forEach(property => element.removeAttribute(property));
}