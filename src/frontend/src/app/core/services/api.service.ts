import axios, { AxiosError } from 'axios'
import type { ApiRequestOptions } from '../models/api-request-options.model'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const method = error.config?.method?.toUpperCase() || 'UNKNOWN'
    const url = error.config?.url || ''

    console.error(`[API Error] ${method} ${url}`, {
      status: error.response?.status,
      data: error.response?.data,
      message: error.message
    })

    return Promise.reject(error)
  }
)

async function request<T>(
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH',
  url: string,
  options?: ApiRequestOptions
  ) : Promise<T> {
  try {
    const response = await apiClient.request<T>({
      method,
      url,
      data: options?.body,
      params: options?.query,
    })
    return response.data
  } catch (error: any) {
    throw error
  }
}

export const api = {
  get: <T>(url: string, options?: ApiRequestOptions) =>
    request<T>('GET', url, options),
  
  post: <T>(url: string, options?: ApiRequestOptions) =>
    request<T>('POST', url, options),

  put: <T>(url: string, options?: ApiRequestOptions) =>
    request<T>('PUT', url, options),

  patch: <T>(url: string, options?: ApiRequestOptions) =>
    request<T>('PATCH', url, options),

  delete: <T>(url: string, options?: ApiRequestOptions) =>
    request<T>('DELETE', url, options),
}