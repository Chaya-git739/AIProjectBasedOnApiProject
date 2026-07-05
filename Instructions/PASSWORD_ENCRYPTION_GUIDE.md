# Password Encryption (BCrypt)

## Overview
Passwords are hashed using BCrypt before being stored in the database.

## Flow
- Register: hash password using BCrypt
- Login: verify password using BCrypt

## Dependency
BCrypt.Net-Next

## Important Note
Existing users with plain-text passwords must be re-seeded or updated.