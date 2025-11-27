import { computed, type Ref } from "vue"

const MIN_YEAR = 1800
const CURRENT_YEAR = new Date().getFullYear()

const ALL_YEARS = Array.from(
  { length: CURRENT_YEAR - MIN_YEAR + 1 },
  (_, i) => CURRENT_YEAR - i
)

export function useYearFilterOptions(
  yearFrom: Ref<number | null>, 
  yearTo: Ref<number | null>
) {

  const yearFromOptions = computed(() => {
    const maxLimit = yearTo.value
    
    if (maxLimit === null) {
      return ALL_YEARS
    }

    return ALL_YEARS.filter(year => year <= maxLimit)
  })

  const yearToOptions = computed(() => {
    const minLimit = yearFrom.value
    
    if (minLimit === null) {
      return ALL_YEARS
    }
    
    return ALL_YEARS.filter(year => year >= minLimit)
  })

  return {
    yearFromOptions,
    yearToOptions
  }
}