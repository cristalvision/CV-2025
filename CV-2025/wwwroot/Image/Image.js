var Image = {

    name: null, contentType: null, width: null, height: null, length: 0, ZoomPan: [1], target: {},

    async Upload(file) {

        const formData = new FormData();
        formData.append("name", "Pomegranate");
        formData.append("file", file);

        let response = await fetch('/Image/Upload', { method: "POST", body: formData });
        response = await response.json();

        Image.name = response.name;
        Image.width = response.width;
        Image.height = response.height;
        Image.length = response.length;
        Image.contentType = response.contentType;

        return response['response'];
    },

    Display() {

        const frame = document.getElementById('Frame');
        frame.innerHTML = null;

        const source = ('Image/Display?Name=' + this.name + '&contentType=' + this.contentType + '&length=' + this.length).replaceAll('+', '%2b');
        this.target = Object.assign(document.createElement('img'), { id: 'Bitmap', src: source, draggable: false });

        (this.width > this.height) ? this.target.style.width = '100%' : this.target.style.height = '100%';

        this.target.addEventListener('wheel', this.Zoom);

        this.target.addEventListener('mousedown', event => {

            let translate = window.getComputedStyle(event.target).translate;
            translate = (translate == 'none') ? '0px, 0px' : translate.replace(' ', ', ');
            const translateX = new DOMMatrix('translate(' + translate + ')').e;
            const translateY = new DOMMatrix('translate(' + translate + ')').f;

            this.ZoomPan[1] = translateX - event.clientX;
            this.ZoomPan[2] = translateY - event.clientY;

            this.target.addEventListener('mousemove', this.Pan);
        });

        this.target.addEventListener('mouseup', () => {
            this.target.removeEventListener('mousemove', this.Pan);
        });

        frame.append(this.target);
        Image.target = this.target;
    },

    Zoom(event) {

        let level = new WebKitCSSMatrix(window.getComputedStyle(this).transform).a;
        level = (event.deltaY == 100) ? level -= 2 : level += 2;

        if (1 > level || level > 20) return;

        this.style.transform = 'scale(' + level + ')';

        Image.ZoomPan[0] = level;
        Image.ZoomPan[3] = this.offsetWidth * ((level + 1) / 2 - 1) - this.offsetLeft;
        Image.ZoomPan[4] = this.offsetHeight * ((level + 1) / 2 - 1);

        if (event.deltaY == -100) return;

        //┌──────────────────────Stick to the edge──────────────────────┐
        let translate = window.getComputedStyle(event.target).translate;
        translate = (translate == 'none') ? '0px, 0px' : translate.replace(' ', ', ');

        let translateX = new DOMMatrix('translate(' + translate + ')').e;
        let translateY = new DOMMatrix('translate(' + translate + ')').f;
        const factorX = (translateX > 0) ? 1 : -1;
        const factorY = (translateY > 0) ? 1 : -1;

        if (Math.abs(translateX) > Image.ZoomPan[3]) translateX = Image.ZoomPan[3] * factorX;
        if (Math.abs(translateY) > Image.ZoomPan[4]) translateY = Image.ZoomPan[4] * factorY;

        if (level == 1) { translateX = 0; translateY = 0; }
        this.style.translate = translateX + 'px ' + translateY + 'px';
        //└──────────────────────Stick to the edge──────────────────────┘

    },

    Pan(event) {

        if (Image.ZoomPan[0] == 1) return;

        const referenceX = Image.ZoomPan[1];
        const referenceY = Image.ZoomPan[2];
        const referenceW = Image.ZoomPan[3];
        const referenceH = Image.ZoomPan[4];

        if (event.clientY + referenceY - referenceH > 0) return;//Prevent overflow top
        if (event.clientY + referenceY + referenceH < 0) return;//Prevent overflow bottom
        if (event.clientX + referenceX - referenceW > 0) return;//Prevent overflow left
        if (event.clientX + referenceX + referenceW < 0) return;//Prevent overflow right

        this.style.translate = (event.clientX + referenceX) + 'px ' + (event.clientY + referenceY) + 'px';
    }
}