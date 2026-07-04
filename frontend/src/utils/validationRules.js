import * as z from "zod";

export const userNameOrEmailRule = z
  .string()
  .trim()
  .min(1, "Введіть ім'я користувача або email");

export const userNameRule = z
  .string()
  .trim()
  .min(1, "Введіть ім'я користувача")
  .regex(
    /^[a-zA-Z0-9._]+$/,
    "Ім'я користувача може містити лише латинські літери, цифри, крапки та підкреслення"
  );

export const emailRule = z
  .string()
  .trim()
  .min(1, "Введіть email")
  .email("Невірний формат email");

export const passwordRule = z
  .string()
  //.trim() - is not used for passwords (conflicts with possible spaces in password intentionally)
  .min(1, "Введіть пароль")
  .min(6, "Мінімум 6 символів")
  .refine((val) => val.trim().length > 0, {
    message: "Пароль не може складатися лише з пробілів",
  });

export const confirmPasswordRule = z
  .string()
  .min(1, "Підтвердіть пароль")
  .refine((val) => val.trim().length > 0, {
    message: "Підтвердіть пароль",
  });

export const withConfirmPassword = (schema) => 
  schema.refine((data) => data.password === data.confirmPassword, {
    message: "Паролі не збігаються",
    path: ["confirmPassword"],
  });