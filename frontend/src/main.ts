import './assets/main.css';
import { createApp } from 'vue';
import { createPinia } from 'pinia';
import axiosInstance from './plugins/axios';

import App from './App.vue';
import router from './router';

// Import Font Awesome libraries
import { library } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// Import specific icons (add the ones you need)
import { faChevronLeft, faBars, faHome, faCog ,faRightFromBracket} from '@fortawesome/free-solid-svg-icons';

// Add icons to the library
library.add(faChevronLeft, faBars, faHome, faCog,faRightFromBracket);

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.config.globalProperties.$axios = axiosInstance;

// Register Font Awesome globally
app.component('font-awesome-icon', FontAwesomeIcon);

app.mount('#app');
