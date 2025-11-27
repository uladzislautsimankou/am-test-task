import { createApp } from 'vue'
import App from './app/App.vue'
import router from './app/router'
import './styles.scss'

const app = createApp(App)

app.use(router)

app.mount('#app')
