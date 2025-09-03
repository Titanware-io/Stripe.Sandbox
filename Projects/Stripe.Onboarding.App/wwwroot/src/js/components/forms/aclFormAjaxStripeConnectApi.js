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
        element: 'auBankAccount',
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
                    //if (this.formData.fields[i].name != 'Name')
                    //    this.formData.fields[i].hidden = true;
                }
            }

            params.country = 'Australia'
            if (params.country == 'Australia') {
                this.element = 'auBankAccount';
            }
            //this.jwkJson = JSON.parse(params.jwkJson);
            this._mxFetch_setValues(params);

            this.publicKey = params.publicKey;
            this.secretKey = params.secretKey;

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
                //payment_method_types: [ "iban", "sepa_debit", "au_becs_debit", ]
            }
            this.cardElement = await this.$store.svcStripe.createElement("auBankAccount", options);

            this.cardElement.on('change', function (event) {
                if (event.complete) {
                    // enable payment button
                    console.log(event);
                }
            });
            this.cardElement.mount('#payment-element');
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
                self.$store.svcStripe.createToken('bank_account', {
                    account_holder_name: submittedData.accountHolder,
                    account_holder_type: 'individual',
                    account_number: submittedData.accountNumber, // Stripe test account number
                    routing_number: submittedData.routingNumber, // Stripe test routing number
                    currency: submittedData.currency,
                    country: submittedData.country
                })
                    .then(async function (data) {
                        // Send token to server
                        console.log(data);
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
                    externalId: tokenData.bank_account.id,
                    type: tokenData.bank_account.funding,
                    name: tokenData.bank_account.account_holder_name,
                    scheme: tokenData.bank_account.brand,
                    accountNumber: tokenData.bank_account.last4,
                    routing_number: tokenData.bank_account.RoutingNumber,
                    status: tokenData.bank_account.status,
                    currency: tokenData.bank_account.currency,
                    country: tokenData.bank_account.country,
                    bankName: tokenData.bank_account.bank_name
                };


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
                    <div id="payment-element" ></div>

                    <div x-data="aclFormAjax(formData)" @onfieldchange="onFieldChange"></div>

                    <span x-data="{ init() { this.loadForm() } }"></span>

                </div>
            `
            this.$nextTick(() => { this.$root.innerHTML = html });
        },
    }
}