 
import { mxEvent, mxFetch, mxService, mxToast } from '/src/js/mixins/index.js';


//https://docs.stripe.com/js/initializing
export default function (settings) {
    return {
        ...mxEvent(settings),
        ...mxFetch(settings),
        ...mxService(settings),
        ...mxToast(settings),
        // PROPERTIES
        stripe: null,
        elements: null,
        self: null,
        async init() {
            this.self = this;
        },
        // GETTERS
        // METHODS
        loadStripe(apiKey, clientSecret) {

            this.stripe = Stripe(apiKey);

            this.elements = this.stripe.elements({
                clientSecret: clientSecret,
            });
        },
        async createToken(options, data) {
            return this.stripe.createToken(options, data);
        },
        async loadElement(element, options) {
            return this.elements.create(element, options);
        }
    }
}