export default function (data) {
    return {
        // PROPERTIES
        mxContent_item: {},
        mxContent_items: [],
        mxContent_img: '',
        mxContent_icon: '',
        mxContent_btn: null,
        mxContent_title: '',
        mxContent_subtitle: '',
        mxContent_text: '',
        mxContent_subtext: '',
        init() {
        },
        // GETTERS
        get mxContent_titleClass() { return 'w-full text-3xl font-bold ' },
        get mxContent_subtitleClass() { return 'w-full text-2xl text-gray-500' },
        get mxContent_textClass() { return 'text-lg text-gray-500' },
        // METHODS
        _mxContent_setValues(params) {
            if (!params) return;
            this.mxContent_btn = params.btn;
            this.mxContent_items = params.items;
            this.mxContent_icon = params.icon;
            this.mxContent_img = params.img;
            this.mxContent_title = params.title;
            this.mxContent_subtitle = params.subtitle;
            this.mxContent_text = params.text;
            this.mxContent_subtext = params.subtext;
        },
    }
}