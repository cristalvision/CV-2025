var Process = {

    ws: {},

    construct() {
        ConstructWS('wss://' + Home.address + '/api/WebSocket');
    },

    async StartOCR() {

        Process.ws.send(JSON.stringify(['Start', Image.name]));
        Home.progress.create();
        document.getElementById('Frame').append(Home.progress.div);

        const source = await ServerResponse('Start');
        Input.target.src = source.replaceAll('+', '%2b')
        Home.progress.hide();
    }

    /*ws.onmessage = function (evt) {
        console.log(JSON.parse(event.data));
    }*/
}

Process.construct();