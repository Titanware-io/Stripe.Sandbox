import { mxTable, mxFetch, mxEvent } from '/src/js/mixins/index.js';

export default function (params) {
    return {
        ...mxTable(params),
        ...mxEvent(params),
        ...mxFetch(params),
        // PROPERTIES
        expanded: false,
        queryText: null,
        // INIT
        async init() {
            this._mxTable_setValues(params);
            this.setValues(params);
            this.render();
            await this.query();
        },
        // GETTERS
        headerKeys() {
            // return this.mxTable_headers.map((x) => x.text);
        },
        // METHODS
        setValues(params) {
            this.mxFetch_url = params ? params.url : '';
        },
        setEvents() {
            const self = this;
        },
        getClass(item, i) {
            const header = this.mxTable_headers.filter(x => x.value == i)[0]
            if (header == null) return 'px-6 py-4';
            return header.class;
        },
        getRow(item, header) {
            const headerKey = header.value;
            return this.getColValue(item[headerKey], header);
        },
        getColValue(keyValue, header) {
            if (!keyValue) return '';
            if (!header) return keyValue.key || ''; 
            switch (header.headerType) {
                case this.mxTable_headerTypeText:
                    // Overrode existing
                    return keyValue.value;
                case this.mxTable_headerTypeLink:
                    return `<a href="${keyValue.value}" class="font-medium text-blue-600 dark:text-blue-500 hover:underline">${keyValue.key}</a>`;
                case this.mxTable_headerTypeButtons:
                    if (keyValue.value == null) return '';
                    var data = { items: keyValue.value }
                    return `<div class='' x-data='aclDropdownMenuButton(${JSON.stringify(data)})'></div>`
                case this.mxTable_headerTypeNumber:
                    return keyValue.key;
                case this.mxTable_headerTypeDate:
                    return keyValue.key;
                case this.mxTable_headerTypeTag:
                    return `<div class="rounded-md bg-blue-600 py-0.5 px-2.5 border border-transparent text-sm text-white transition-all shadow-sm">${keyValue.value}</div>`;
                default:
                    return keyValue.key;
            }
        },
        getKey(item, i) {
            if (!item) return '_';
            return item.text + ':' + i;
        },
        getText(item) {
            if (!item) return '';
            return item.text;
        },
        getValue(item, headerKey) {
            if (!item) return '';
            if (!headerKey) return '';
            return item[headerKey];
        },
        toCamelCase(str) {
            if (!str) return;
            return str.toLowerCase().replace(/[^a-zA-Z0-9]+(.)/g, (m, chr) => chr.toUpperCase());
        },
        async updatePage(page) {
            this.mxTable_page = page - 1;
            await this.query();
        },
        async nextPage() {
            if (this.mxTable_page == this.mxTable_pages-1) return;
            this.mxTable_page++;
            await this.query();
        },
        async previousPage() {
            if (this.mxTable_page == 0) return;
            this.mxTable_page--;
            await this.query();
        },
        async query() {
            // else
            if (!this.mxFetch_url) return;
            this.mxFetch_loading = true;
            try {
                const payload = {
                    page: this.mxTable_page || 0,
                    pageSize: this.mxTable_pageSize || 10,
                    pages: this.mxTable_pages || 0,
                    query: this.mxTable_query || {
                        Name: this.queryText || ''
                    }
                }
                const result = await this.$fetch.POST(this.mxFetch_url, payload);

                if (result != null && result.status == 200) {
                    result.items = result.data;
                    this._mxTable_setPaginationValues(result)
                }
                else {

                }
            } catch (e) {
                console.log(e);
            }
            this.mxFetch_loading = false;
        },
        render() {
            const html = `
                <div class="overflow-auto">
                    <div class="flex flex-column sm:flex-row flex-wrap space-y-4 sm:space-y-0 items-center justify-between pb-4">

                        <label for="table-search" class="sr-only">Search</label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 rtl:inset-r-0 rtl:right-0 flex items-center ps-3 pointer-events-none">
                                <svg class="w-5 h-5 text-gray-500 dark:text-gray-400" aria-hidden="true" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd"></path></svg>
                            </div>
                            <input :value="queryText" x-model="queryText" type="text" @change="query" id="table-search" class="block p-2 ps-10 text-sm text-gray-900 border border-gray-300 rounded-lg w-80 bg-gray-50 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500" placeholder="Search for items">
                        </div>
                    </div>

                    <!-- Loader -->
                    <div x-show="mxFetch_loading" x-data="aclCommonProgress()"></div>
                    <!--Table-->
                    <table class="table-auto overflow-x-scroll w-full text-sm text-left rtl:text-right text-gray-500 dark:text-gray-400">

                    <!--Headers-->
                        <thead class="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
                            <tr>
                                <template x-if="mxTable_multiSelect">
                                    <th scope="col" class="p-4">
                                        <div class="flex items-center">
                                            <input id="checkbox-all-search" type="checkbox" class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 dark:focus:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600">
                                            <label for="checkbox-all-search" class="sr-only">checkbox</label>
                                        </div>
                                    </th>
                                </template>
                                <template x-for="(item, i) in mxTable_headers">
                                    <th scope="col" class="px-1 py-2" x-text="getText(item)"></th>
                                </template>
                            </tr>
                        </thead>

                        <!--Rows-->
                        <tbody>
                            <template x-for="item in mxTable_items">
                                <tr class="bg-white border-b dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600">
                                    <!--Checkbox-->
                                    <template x-if="mxTable_multiSelect">
                                        <td class="w-4 p-4">
                                            <div class="flex items-center">
                                                <input id="checkbox-table-search-1" type="checkbox" class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 dark:focus:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600">
                                                <label for="checkbox-table-search-1" class="sr-only">checkbox</label>
                                            </div>
                                        </td>
                                    </template>
                                    <!--Row-->
                                    <template x-for="(header, i) in mxTable_headers" >
                                        <td x-html="getRow(item, header, i)" class="px-1" :class="getClass(item, i)" :key="i"></td>
                                    </template>
                                </tr>
                            </template> 
                        </tbody>
                    </table>
                   
                    <nav class="flex px-2 py-2 items-center flex-column flex-wrap md:flex-row justify-between pt-4" aria-label="Table navigation">
                        <div class="justify-center align-center inline-flex -space-x-px rtl:space-x-reverse text-sm h-8">
                            <span class="">
                                Showing
                                <span class="font-semibold text-gray-900 dark:text-white" x-text="(mxTable_page + 1) + ' - '+ ((mxTable_page + 1) * mxTable_pageSize)"></span>
                                of
                                <span class="font-semibold text-gray-900 dark:text-white" x-text="mxTable_total"></span>
                            </span>
                        </div>
                        <ul class="inline-flex -space-x-px rtl:space-x-reverse text-sm h-8">
                            <li x-show="mxTable_page > 0">
                                <a @click="await previousPage()" class="flex cursor-pointer  items-center justify-center px-3 h-8 ms-0 leading-tight text-gray-500 bg-white border border-gray-300 rounded-s-lg hover:bg-gray-100 hover:text-gray-700 dark:bg-gray-800 dark:border-gray-700 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white">Previous</a>
                            </li>
                            <template x-for="page in mxTable_totalPages">
                                <li>
                                    <a @click="await updatePage(page)"
                                    :aria-current="mxTable_page+1 == page ? 'page' : null"
                                    :class="mxTable_page+1 == page
                                        ? 'flex items-center justify-center cursor-pointer px-3 h-8 text-blue-600 border border-gray-300 bg-blue-50 hover:bg-blue-100 hover:text-blue-700 dark:border-gray-700 dark:bg-gray-700 dark:text-white'
                                        : 'flex items-center justify-center cursor-pointer px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 hover:bg-gray-100 hover:text-gray-700 dark:bg-gray-800 dark:border-gray-700 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white'"
                                    x-text="page"></a>
                                </li>
                            </template> 
                            <li x-show="mxTable_page < mxTable_totalPages-1">
                                <a @click="await nextPage()"class="flex items-center cursor-pointer justify-center px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 rounded-e-lg hover:bg-gray-100 hover:text-gray-700 dark:bg-gray-800 dark:border-gray-700 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white">Next</a>
                            </li>
                        </ul>
                    </nav>
                   
                </div>
            `
            this.$nextTick(() => { this.$root.innerHTML = html });
        }
    }
}