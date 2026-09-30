import '@fontsource-variable/inter'
import React from 'react'
import ReactDOM from 'react-dom/client'
import { App } from './app/App'
import './app/i18n'
import './app/i18n.invoicePdf'
import './app/i18n.ui'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
