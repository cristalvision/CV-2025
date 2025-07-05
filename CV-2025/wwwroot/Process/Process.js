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

        const scale = 1;
        const xAxis = 774 - 175;
        const yAxis = 434;

        Image.ZoomPan[0] = scale;
        Image.ZoomPan[1] = xAxis;
        Image.ZoomPan[2] = yAxis;

        //Input.target.style.transform = 'scale(' + scale + ', ' + scale + ') translate(' + xAxis + 'px, ' + yAxis + 'px)';//Licenta 2009
        //Input.target.style.transform = 'scale(5, 5) translate(210px, 360px)';//Eminescu
        //Prima data sa stie sa alinieze imaginea indiferent de dimensiuni, inclusiv pe dimensiuni egale
        //Apoi sa modific doar latimea, inaltimea a ramana identica pentru fiecare imagine

        document.getElementsByTagName('process')[0].style.display = 'none';
        document.getElementsByTagName('output')[0].style.display = 'inline-flex';
    }

    /*ws.onmessage = function (evt) {
        console.log(JSON.parse(event.data));
    }*/
}

Process.construct();
