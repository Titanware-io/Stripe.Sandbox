import { mxContent, mxForm, mxFetch, mxField } from '/src/js/mixins/index.js';

export default function (params) {
    return {
        ...mxContent(params),
        ...mxForm(params),
        ...mxField(params),
        ...mxFetch(params),
        // PROPERTIES
        header: '',
        publicKey: null,
        secretKey: null,
        cardElement: null,

        loading: false,
        // INIT
        async init() {
            //override submit button
            //get values from card number, security code, expMonth and year
            //send to hidden fields
            //generate token
            //display errors
            this.formData = params;
            if (this.formData != null) {
                for (var i = 0; i < this.formData.fields.length; i++) {
                    // Hide all fields but the name
                    if (this.formData.fields[i].name != 'Name')
                        this.formData.fields[i].hidden = true;
                }
            }

            //this.jwkJson = JSON.parse(params.jwkJson);
            this._mxFetch_setValues(params);

            this.publicKey = params.publicKey;
            this.secretKey = params.secretKey;
            this.cardCcvSecretKey = params.cardCcvSecretKey;
            this.cardNumberSecretKey = params.cardNumberSecretKey

            this.loadStripe(params.publicKey, params.secretKey);

            this.formData.submit = this.onSubmit;
            this.render();
        },
        // GETTERS
        // METHODS
        async loadStripe(apiKey, clientSecret) {
            this.$store.svcStripe.loadStripe(apiKey, clientSecret)
        },
        async loadForm() {
            var options = {
                classes: {
                    base: ' w-full h-full max-w-md mx-auto p-4'
                },
                value: {
                    name: 'qwddqw',
                },
                hidePostalCode: true
            }
            this.cardElement = await this.$store.svcStripe.createElement("card", options);

            this.cardElement.on('change', function (event) {
                if (event.complete) {
                    // enable payment button
                    console.log(event);
                }
            });
            this.cardElement.mount('#card-element');
        },
        onFieldChange(ev) {
            const field = ev.detail;
            switch (field.name) {
                case "Name":
                    //this.cardData[field.name] = field.value;
                    break;
                default:
                    return;
            }
        },
        async onSubmit(data) {
            this.loading = true;
            await this.createToken(data, this.formData);
        },
        //https://stackoverflow.com/questions/61501493/send-add-cvv-cvn-field-on-cybersource-flex-microform
        //https://developer.cybersource.com/docs/cybs/en-us/digital-accept-flex-api/developer/ctv/rest/flex-api/microform-integ-v2/api-reference-v2.html

        // Create Stripe payment element
        // Tokenize
        async createToken(submittedData, formData) {
            const self = this;
            try {
                self.$store.svcStripe.createToken(self.cardElement, {})
                    .then(async function (data) {
                    // Send token to server
                        await self.saveToken(data.token, submittedData, self.formData)
                    });
                this.loading = false;
            } catch (e) {
                console.log(e);
                this.loading = false;
            }
        },

        async saveToken(tokenData, submittedData, formData) {
            // else
            this.loading = true;
            try { 

                let payload = {
                    ...submittedData,
                    token: tokenData.id,
                    ip: tokenData.client_ip,
                    externalId: tokenData.card.id,
                    type: tokenData.card.funding,
                    name: submittedData.name,
                    scheme: tokenData.card.brand,
                    maskedNumber: `**${tokenData.card.last4}`,
                    ExpMonth: tokenData.card.exp_month,
                    ExpYear: tokenData.card.exp_year,
                } 
                const result = await this.$fetch.POST(formData.action, payload);

                if (this.mxForm_event) {
                    this.$dispatch(this.mxForm_event, result)
                }
            } catch (e) {
                console.log(e);
            }
            this.loading = false;
        },
        overrideSubmit(e) {
            e.preventDefault();
            return;
        },
        render() {
            const html = `
               
                <div>
                    <div
                        :class="mxForm_class"
                        x-show="loading"
                        x-data="aclCommonProgress({})"></div>

                    <!-- Stripe card -->
                    <div id="card-element"></div>

                    <div x-data="aclFormAjax(formData)" @onfieldchange="onFieldChange"></div>

                    <span x-data="{ init() { this.loadForm() } }"></span>

                </div>
            `
            this.$nextTick(() => { this.$root.innerHTML = html });
        },
    }
}