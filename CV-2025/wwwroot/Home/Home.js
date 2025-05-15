var Input = {};

Home = {

    address: 'localhost:7234',

    async UploadDisplay(files) {
        //(files[0].name)//Check file extension

        const file = files[0];
        Input = Object.create(Image);
        const response = await Input.Upload(file);

        (response == 'OK') ? Input.Display() : alert(response);
    },

    DisplayDocument(file) {

    },

    DisplayFrame(frameID) {

    },

    progress: {

        div: {}, label: {}, bar: {},

        create() {
            this.div = Object.assign(document.createElement('div'), { id: 'Progress' });
            this.label = Object.assign(Object.assign(document.createElement('label'), { textContent: 'Progress', htmlFor: 'ProgressBar' }));
            this.bar = Object.assign(document.createElement('progress'), { id: 'ProgressBar', max: 100 });

            this.div.append(this.label);
            this.div.append(this.bar);
        },

        update(description, value) {
            this.label.textContent = description;
            this.bar.value = value;
        },

        hide() {
            this.div.className = 'hideElement';
        }
    }
}