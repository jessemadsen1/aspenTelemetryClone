variable "api_dbuser" {
  type        = string
  description = "api database user"
}

variable "api_dbpassword" {
  type        = string
  description = "api database password"
  sensitive   = true
}

variable "keycloak_dbuser" {
  type        = string
  description = "keycloak database user"
}

variable "keycloak_dbpassword" {
  type        = string
  description = "keycloak database password"
  sensitive   = true
}

variable "keycloak_admin_username" {
  type        = string
  description = "keycloak_admin_username"
}

variable "keycloak_admin_password" {
  type        = string
  description = "keycloak_admin_password"
  sensitive   = true
}
