 
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
        connect: null,
        connectInstance: null,
        elements: null,
        self: null,
        async init() {
            this.connect = window.StripeConnect || {};
            this.self = this;
        },
        // GETTERS
        // METHODS
        // Load normal stripe js
        loadStripe(apiKey, clientSecret) {
            this.stripe = Stripe(apiKey);

            this.elements = this.stripe.elements({
                clientSecret: clientSecret,
            });
        },
        // load connect stripe js
        loadConnect(apiKey, clientSecret) {
            const self = this;
            self.connect.onLoad = () => {
                self.connectInstance = self.connect.init({
                    // This is your test publishable API key.
                    publishableKey: apiKey,
                    fetchClientSecret: clientSecret,
                }); 
            };
        },
        async createToken(options, data) {
            return this.stripe.createToken(options, data);
        },
        async loadElement(element, options) {
            return this.elements.create(element, options);
        },
        async createConnectElement(elementId, options) {
            return this.connectInstance.create(element);
        },
        // Move to component function
        async loadOnboarding() {
            // onboarding
            const accountOnboarding = this.connect.create('account-onboarding');
            // change
            accountOnboarding.setOnStepChange((stepChange) => {
                console.log(`User entered: ${stepChange.step}`);
            });
        }
    }
}