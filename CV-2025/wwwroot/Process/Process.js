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
        const top = response[1];
        const left = response[2];
        const width = response[3];

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

        //┌───────────────────────Update Database───────────────────────┐
        const charValue = document.getElementById('Char');
        charValue.style.visibility = '';
        charValue.focus();

        const charStyle = document.getElementById('CharStyle');
        charStyle.style.visibility = '';

        const updateBtn = document.getElementById('UpdateDB');
        updateBtn.style.visibility = '';
        updateBtn.onclick = () => {
            const style = charStyle.checked;
            const value = charValue.value;
            Process.ws.send(JSON.stringify(['UpdateDatabase', style, value, width]));
            charStyle.style.visibility = 'hidden';
            updateBtn.style.visibility = 'hidden';
        }
        //└───────────────────────Update Database───────────────────────┘
    }

    /*ws.onmessage = function (evt) {
        console.log(JSON.parse(event.data));
    }*/
}

Process.construct();
