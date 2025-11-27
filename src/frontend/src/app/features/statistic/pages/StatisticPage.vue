<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { api } from '@/app/core/services/api.service'
import type { RecClass } from '../models/rec-class.model';
import type { Filter } from '../models/filter.model';
import type { MeteoriteStatictic } from '../models/meteorite-statistic.model';
import { SortBy } from '../enums/sort-by.enum';
import { SortDir } from '../enums/sort-dir.enum';
import { useYearFilterOptions } from '@/app/core/composables/year-filter-options.composable';

const route = useRoute();
const router = useRouter();

const isPageReady = ref(false)
const isLoading = ref(false)

const recClasses = ref<RecClass[]>([])
const meteoriteStatisticsData = ref<MeteoriteStatictic[]>([])
const filters = reactive<Filter>({
  yearFrom: null,
  yearTo: null,
  recClassId: null,
  namePart: null
})

const sort = reactive({
  by: SortBy.Year,
  dir: SortDir.Asc
})

const validationError = ref<string | null>(null)
const dataFetchError = ref<string | null>(null)

// по-хорошему, это бы на бэке считать, особенно, если пагинация какая-то добавится
// но, в задании про бэк об это ни слова, так что тут оставляю, видимо для чего-то так надо)
const totalStats = computed(() => {
  if (!meteoriteStatisticsData.value.length) return null;

  return meteoriteStatisticsData.value.reduce(
    (acc, item) => {
      acc.count += item.count;
      acc.totalMass += item.totalMass;
      return acc;
    },
    { count: 0, totalMass: 0 }
  );
});

const { yearFromOptions, yearToOptions } = useYearFilterOptions(
  computed(() => filters.yearFrom),
  computed(() => filters.yearTo)
)

const fetchClasses = async () => {
  try {
    recClasses.value = await api.get<RecClass[]>('/meteorites/classes')
  } catch {
    recClasses.value = []
  } finally {
    isPageReady.value = true
  }
}

const fetchMeteoriteStatisticData = async (params: Record<string, any> | undefined) => {
  isLoading.value = true
  dataFetchError.value = null

  try {
    meteoriteStatisticsData.value = await api.get<MeteoriteStatictic[]>('/meteorites/statistic', {
      query: params
    })
  } catch {
    dataFetchError.value = "Не удалось загрузить данные."
    meteoriteStatisticsData.value = []
  } finally {
    isLoading.value = false
  }
}

// минимальная валидация, если кто-то руками в роут вписал не правильно
const isValid = (filters: Filter) => {
  validationError.value = null

  if (filters.yearFrom && filters.yearTo) {
    if (filters.yearFrom > filters.yearTo) {
      validationError.value = 'Год "С" не может быть больше года "По"'
      return false
    }
  }

  return true;
}

const handleSort = (column: SortBy) => {
  if (isLoading.value) return;

  if (sort.by === column) {
    // инвертируем направление
    sort.dir = sort.dir === SortDir.Asc ? SortDir.Desc : SortDir.Asc
  } else {
    sort.by = column
    sort.dir = SortDir.Asc
  }

  onSearch()
}

const onSearch = () => {
  if (!isValid(filters) || isLoading.value) return

  const query: Record<string, any> = {}

  if (filters.yearFrom) query.yearFrom = filters.yearFrom
  if (filters.yearTo) query.yearTo = filters.yearTo
  if (filters.recClassId) query.recClassId = filters.recClassId

  const trimmedName = filters.namePart?.trim()
  if (trimmedName) query.namePart = trimmedName

  query.sortBy = sort.by
  query.sortDir = sort.dir

  router.push({ query })
}

const syncWithUrl = () => {
  const q = route.query

  // парсим урл
  const pYearFrom = Number(q.yearFrom)
  const pYearTo = Number(q.yearTo)
  const pClassId = Number(q.recClassId)

  filters.yearFrom = !isNaN(pYearFrom) ? pYearFrom : null
  filters.yearTo = !isNaN(pYearTo) ? pYearTo : null
  filters.recClassId = !isNaN(pClassId) ? pClassId : null
  filters.namePart = (q.namePart as string) || null

  sort.by  = (q.sortBy !== undefined && Number(q.sortBy) in SortBy)
    ? Number(q.sortBy) as SortBy
    : SortBy.Year

  sort.dir = (q.sortDir !== undefined && Number(q.sortDir) in SortDir)
    ? Number(q.sortDir) as SortDir
    : SortDir.Asc

  if (!isValid(filters)) return

  // формируем чистый объект для API
  const apiParams: Record<string, any> = {
    sortBy: sort.by,
    sortDir: sort.dir,
  }

  if (filters.yearFrom) apiParams.yearFrom = filters.yearFrom
  if (filters.yearTo) apiParams.yearTo = filters.yearTo
  if (filters.recClassId) apiParams.recClassId = filters.recClassId
  if (filters.namePart) apiParams.namePart = filters.namePart

  fetchMeteoriteStatisticData(apiParams)
}

const getSortIcon = (column: SortBy) => {
  if (sort.by !== column) return '↕'
  return sort.dir === SortDir.Asc ? '↑' : '↓'
}

onMounted(() => {
  fetchClasses()
})

watch(
  () => route.query,
  () => syncWithUrl(),
  { immediate: true }
)
</script>

<template>
  <div class="wrapper">
    <div class="stats-page">
      <h1>Статистика по метеоритам</h1>

      <div class="filters-container">
        <form @submit.prevent="onSearch" class="filters">
          <div class="filter-item">
            <select v-model="filters.yearFrom" :class="{ 'invalid': validationError }">
              <option :value="null">Год с</option>
              <option v-for="year in yearFromOptions" :key="year" :value="year">
                {{ year }}
              </option>
            </select>
          </div>

          <div class="filter-item">
            <select v-model="filters.yearTo" :class="{ 'invalid': validationError }">
              <option :value="null">Год по</option>
              <option v-for="year in yearToOptions" :key="year" :value="year">
                {{ year }}
              </option>
            </select>
          </div>

          <div class="filter-item">
            <select v-model="filters.recClassId">
              <option :value=null>Все классы</option>
              <option v-for="recCass in recClasses" :key="recCass.id" :value="recCass.id">
                {{ recCass.name }}
              </option>
            </select>
          </div>

          <div class="filter-item">
            <input v-model.trim="filters.namePart" type="text" placeholder="Название метеорита" />
          </div>

          <button type="submit" :disabled="isLoading">Поиск</button>
        </form>
        
        <p v-if="validationError" class="error-message">{{ validationError }}</p>
      </div>

      <div v-if="isLoading" class="loader">Загрузка...</div>

      <div v-else-if="dataFetchError">{{ dataFetchError }}</div>

      <div v-else-if="meteoriteStatisticsData.length === 0" class="loader">Нет данных по выбранным фильтрам</div>

      <div v-else class="data">
        <table>
          <thead>
              <tr>
                <th @click="handleSort(SortBy.Year)" :class="{ active: sort.by === SortBy.Year, disabled: isLoading }">
                  Год 
                  <span class="sort-icon">{{ getSortIcon(SortBy.Year) }}</span>
                </th>
                <th @click="handleSort(SortBy.Count)" :class="{ active: sort.by === SortBy.Count, disabled: isLoading }">
                  Количество метеоритов 
                  <span class="sort-icon">{{ getSortIcon(SortBy.Count) }}</span>
                </th>
                <th @click="handleSort(SortBy.Mass)" :class="{ active: sort.by === SortBy.Mass, disabled: isLoading }">
                  Суммарная масса 
                  <span class="sort-icon">{{ getSortIcon(SortBy.Mass) }}</span>
                </th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="row in meteoriteStatisticsData" :key="row.year ?? 'unknown'">
                <td>{{ row.year ?? `-` }}</td>
                <td>{{ row.count }}</td>
                <td>{{ row.totalMass }}</td>
              </tr>
            </tbody>

            <tfoot>
              <tr>
                <td>Total</td>
                <td>{{ totalStats!.count }}</td>
                <td>{{ totalStats!.totalMass.toFixed(2) }}</td>
              </tr>
            </tfoot>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
h1 {
  font-weight: 500;
}

.wrapper {
  display: flex;
  width: 100%;
  height: 100%;
  justify-content: center;
  padding: 40px 16px;

  .stats-page {
    display: grid;
    width: 100%;
    max-width: 900px;
    grid-gap: 24px;
  }
}

.filters-container {
  display: grid;
  grid-gap: 8px;
  padding: 16px;
  background: #efefef;
  border: 1px solid #dddddd;

  .filters {
    display: flex;
    width: 100%;
    grid-gap: 8px;

    .filter-item {
      width: 100%;
      display: flex;
      flex-direction: column;

      input, select {
        width: 100%;
        border: 1px solid #dddddd;
        padding: 4px 8px;

        &.invalid {
          border-color: red;
        }
      }
    }

    button {
      padding: 0 16px;
      background-color: #dbeafe;
      border: 1px solid #007bff;
      color: #004085;
      cursor: pointer;
    }
  }

  .error-message {
    color: red;
    font-size: 12px;
  }
}

table {
  width: 100%;
  margin-bottom: 20px;
  border: 1px solid #dddddd;
  border-collapse: collapse;

  th {
    font-weight: bold;
    padding: 5px;
    background-color: #efefef;
    border: 1px solid #dddddd;
    font-weight: 600;
    cursor: pointer;
    user-select: none;

    &.active {
      background-color: #dbeafe;
      color: #004085;
      border-bottom: 2px solid #007bff;
    }

    &.disabled {
      pointer-events: none;
      opacity: 0.6;
    }

    .sort-icon {
      display: inline-block;
      margin-left: 5px;
      font-size: 12px;
      width: 16px;
    }
  }

  td {
    border: 1px solid #dddddd;
    padding: 5px;
  }

  tfoot {
    td {
      background-color: #efefef;
      font-weight: 600;
    }
  }
}
</style>