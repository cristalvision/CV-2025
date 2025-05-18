var Process = {

    ws: {},

    construct() {
        ConstructWS('wss://' + Home.address + '/api/WebSocket');
    },

    async StartOCR() {

        Process.ws.send(JSON.stringify(['Start', Image.name]));
        Home.progress.create();
        document.getElementById('Frame').append(Home.progress.div);

        let response = await ServerResponse('Start');
        const source = response[1];
        Input.target.src = source.replaceAll('+', '%2b')
        Home.progress.hide();

        Process.ws.send(JSON.stringify(['DisplayUnwnownChar', null]));
        response = await ServerResponse('DisplayUnwnownChar');
        const top = response[1];//38
        const left = response[2];//421
        
        Image.ZoomPan[0] = 3;
        Image.ZoomPan[1] = 575;
        Image.ZoomPan[2] = 101;
        //Input.target.style.transform = 'scale(5, 5) translate(645px, 362px)';//Licenta 2009
        Input.target.style.transform = 'scale(5, 5) translate(210px, 360px)';//Eminescu
        //Translate: 2 x image width, 2 x image height
    }

    /*ws.onmessage = function (evt) {
        console.log(JSON.parse(event.data));
    }*/
}

Process.construct();
