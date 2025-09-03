import { mxContent, mxForm } from '/src/js/mixins/index.js';

export default function (params) {
	return {
        ...mxContent(params),
        ...mxForm(params),
        // PROPERTIES
        header: '',
        form: {},
        // INIT
        init() {
            this._mxContent_setValues(params);
            this._mxForm_SetValues(params);
            //this.setValues(params);
            this.render();
        },
        // GETTERS
        // METHODS
        setValues(params) {
            this.mxContent_title = params.title;
            this.mxContent_subtitle = params.subtitle;
            this.mxContent_text = params.text;
            this.mxForm_label = params.label || 'Submit';

            this.mxForm_class = params.class || this.mxForm_formPadlessClass;
            this.mxForm_fields = params.fields;
            this.mxForm_method = params.method;
            this.mxForm_action = params.action;
        },
        updateField(ev) {
            const field = ev.detail;
            this._mxForm_SetFieldValue(this.mxForm_fields, field);
        },
        onChange(field) {
        },
        render() {
            const html = `
                <div :class="mxForm_class">
                    <template x-if="mxContent_title">
                        <h3 :class="mxForm_titleClass" x-text="mxContent_title"></h3>
                    </template>
                    <template x-if="mxContent_text">
                        <p :class="mxForm_textClass" x-text="mxContent_text"></p>
                    </template>
                    <form novalidate :method="mxForm_method" :action="mxForm_action" method="post" class="group relative w-full space-y-8">
                        <!--Fields-->
                        <div x-data="aclFormFieldset({ fields: mxForm_fields })" @onfieldchange="updateField"></div>
                        <!--Response message-->
                        <template x-if="mxForm_response">
                            <p :class="mxForm_responseClass" x-text="mxForm_response"></p>
                        </template>
                        <!--Submit-->
                        <div class="relative">
                             <input role="button" type="submit" :value="mxForm_label" :class="mxForm_submitClass + ' '+ mxForm_submitInvalidClass" />
                        </div>
                    </form>
                </div>
            `
            this.$nextTick(() => { this.$root.innerHTML = html });
      },
    }
}