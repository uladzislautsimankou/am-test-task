import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/stats', // раз других страниц нет, то просто редиректим сразу сюда
    },
    {
      path: '/stats',
      name: 'stats',
      component: () => import('@/app/features/statistic/pages/StatisticPage.vue'),
    },
  ],
})

export default router
