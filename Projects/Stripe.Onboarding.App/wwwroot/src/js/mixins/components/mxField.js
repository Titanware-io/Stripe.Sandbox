export default function (data) {
    return {
        mxField_inputClass: 'block w-full px-4 py-4 text-xl placeholder-gray-400 bg-gray-200 rounded-lg focus:outline-none focus:ring-4 focus:ring-blue-600 focus:ring-opacity-50',
        mxField_inputDisabledClass: 'block w-full px-4 py-4 text-xl placeholder-gray-200 text-gray-500 bg-gray-100 rounded-lg focus:outline-none focus:ring-4 focus:ring-blue-600 focus:ring-opacity-50',
        // PROPERTIES
        mxField_type: 'text',
        mxField_placeholder: null,
        mxField_cssClass: null,
        mxField_id: null,
        mxField_name: null,
        mxField_min: null,
        mxField_max: null,
        mxField_helper: null,
        mxField_multiple: null,
        mxField_accept: '',
        mxField_items: [],
        mxField_rows: 1,
        mxField_class: null,
        mxField_disabled: null,
        mxField_icon: null,
        mxField_value: null,
        mxField_data: null,
        mxField_required: null,
        mxField_readOnly: null,
        mxField_autocomplete: null,
        mxField_ariaInvalid: null,
        mxField_invalidText: null,
        mxField_areaDescribedBy: null,
        mxField_pattern: null,
        init() {
        },
        // GETTERS  
        get mxField_getInputClass() { return 'block w-full px-4 py-4 text-xl placeholder-gray-400 bg-gray-200 rounded-lg focus:outline-none focus:ring-4 focus:ring-blue-600 focus:ring-opacity-50' },
        get mxField_getFileClass() { return 'block w-full py-0 text-sm text-gray-900 border border-gray-300 rounded-lg cursor-pointer bg-gray-50 dark:text-gray-400 focus:outline-none dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400' },

        get mxField_inputInvalidClass() { return 'invalid:[&:not(:placeholder-shown):not(:focus)]:border-red-500 ' },
        get mxField_inputEmailRegex() { return '[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$' },
        get mxField_inputJsonRegex() { return '!(/[^,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t]/$' },
        // METHODS

        _mxField_GetFieldClass() {
            let cssClass = this.mxField_disabled ? this.mxField_inputDisabledClass : this.mxField_inputClass;
            if (this.mxField_class) {
                cssClass = this.mxField_class;
            }
            if (!!this.mxField_icon && !this.mxField_disabled) return `${cssClass} ps-10 p-2.5`;
            return cssClass;
        },
        _mxField_change(id, name, data) {
            const field = {
                id: id,
                name: name,
                value: data
            }
            this.$dispatch('oninputchange', field)
        },
        _mxField_onChange(data) {
            this._mxField_change(
                this.mxField_id,
                this.mxField_name,
                data || this.mxField_value)
        },
        _mxField_setValues(params) {
            this.mxField_type = params.type || 'text';
            this.mxField_placeholder = params.placeholder;
            this.mxField_cssClass = params.cssClass;
            this.mxField_id = params.id;
            this.mxField_name = params.name;
            this.mxField_min = params.min;
            this.mxField_max = params.max;
            this.mxField_icon = params.icon;
            this.mxField_invalidText = params.invalidText;
            this.mxField_multiple = params.multiple;
            this.mxField_accept = params.accept;
            this.mxField_disabled = params.disabled;
            this.mxField_class = params.class;
            this.mxField_hidden = params.hidden;
            this.mxField_value = params.value || '';
            this.mxField_data = params.data;
            this.mxField_required = params.required;
            this.mxField_readOnly = params.readOnly;
            this.mxField_autocomplete = params.autocomplete;
            this.mxField_ariaInvalid = params.ariaInvalid;
            this.mxField_areaDescribedBy = params.areaDescribedBy;
            this.mxField_pattern = params.pattern;
        },
        _mxField_GetFilePreview(file) {
            return (typeof file == 'string') ? file : URL.createObjectURL(file)
        },
        _mxField_ConvertItemStringToObject(x) {
            return {
                key: x,
                value: x,
                disabled: false
            }
        },
    }
}