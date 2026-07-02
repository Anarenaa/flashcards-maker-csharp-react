import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import RegisterPage from "./RegisterPage";
import { describe } from "vitest";

describe("Form and local validation", () => {
  let container;
  let usernameInput;
  let emailInput;
  let passwordInput;
  let confirmPasswordInput;
  let submitButton;

  beforeEach(() => {
    const rendered = render(
      <BrowserRouter>
        <RegisterPage />
      </BrowserRouter>
    );
    container = rendered.container;

    usernameInput = container.querySelector('input[name="userName"]');
    emailInput = container.querySelector('input[name="email"]');
    passwordInput = container.querySelector('input[name="password"]');
    confirmPasswordInput = container.querySelector('input[name="confirmPassword"]');

    submitButton = container.querySelector('button[type="submit"]');
  });

  it("should load with empty inputs and no error messages initially", () => {
    const errorInputs = container.querySelectorAll(".text-danger");

    expect(usernameInput.value).toBe("");
    expect(emailInput.value).toBe("");
    expect(passwordInput.value).toBe("");
    expect(confirmPasswordInput.value).toBe("");
    expect(errorInputs).toHaveLength(0);
  });

  it("should allow the user to fill out the form fields successfully", () => {
    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });

    expect(usernameInput.value).toBe("ananas");
    expect(emailInput.value).toBe("ananas@gmail.com");
    expect(passwordInput.value).toBe("Password123!");
    expect(confirmPasswordInput.value).toBe("Password123!");
  });

  it("should display errors for all empty fields on submit", () => {
    fireEvent.click(submitButton);

    const errorMessages = container.querySelectorAll(".text-danger");
    expect(errorMessages.length).toBeGreaterThanOrEqual(4);

    const errorTexts = Array.from(errorMessages).map(el => el.textContent);
    expect(errorTexts).toContain("Введіть ім'я користувача");
    expect(errorTexts).toContain("Введіть email");
    expect(errorTexts).toContain("Введіть пароль");
    expect(errorTexts).toContain("Підтвердіть пароль");
  });

  it("should display invalid email format error", () => {
    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "invalid-email-format" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });

    fireEvent.click(submitButton);

    const errorMessages = container.querySelectorAll(".text-danger");
    const errorTexts = Array.from(errorMessages).map(el => el.textContent);
    
    expect(errorTexts).toContain("Невірний формат email");
  });

  it("should display error message when passwords do not match", async () => {
    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Different123!" } });

    fireEvent.click(submitButton);

    const errorMessages = container.querySelectorAll(".text-danger");
    const errorTexts = Array.from(errorMessages).map(el => el.textContent);
    
    expect(errorTexts).toContain("Паролі не збігаються");
  });
});

describe("API interaction (MSW)", () => {
  let container;
  let usernameInput;
  let emailInput;
  let passwordInput;
  let confirmPasswordInput;
  let submitButton;

  beforeEach(() => {
    const rendered = render(
      <BrowserRouter>
        <RegisterPage />
      </BrowserRouter>
    );
    container = rendered.container;
    usernameInput = container.querySelector('input[name="userName"]');
    emailInput = container.querySelector('input[name="email"]');
    passwordInput = container.querySelector('input[name="password"]');
    confirmPasswordInput = container.querySelector('input[name="confirmPassword"]');
    submitButton = container.querySelector('button[type="submit"]');
  });

  it("should successfully register and redirect to login on 200 OK", async () => {
    server.use(
      http.post("*/api/auth/register", () => {
        return HttpResponse.json({ message: "Реєстрація та вхід успішні" });
      })
    );

    const fakeLocation = { href: "http://localhost:5173/register" };
    vi.stubGlobal("location", fakeLocation);

    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
    
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(fakeLocation.href).toBe("/main");
    });
    vi.unstubAllGlobals();
  });
  
  it("should display error message when username already exists", async () => {
    server.use(
      http.post("*/api/auth/register", () => {
        return HttpResponse.json(
          {
            UserName: ["Користувач з таким іменем вже існує"]
          },
          { status: 400 }
        );
      })
    );

    fireEvent.change(usernameInput, { target: { value: "existing_user" } });
    fireEvent.change(emailInput, { target: { value: "new_email@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorText = container.querySelector(".text-danger");
      expect(errorText.textContent).toContain("іменем вже існує");
    });
  });

  it("should display error message when email already exists", async () => {
    server.use(
      http.post("*/api/auth/register", () => {
        return HttpResponse.json(
          {
            Email: ["Користувач з таким email вже існує"]
          },
          { status: 400 }
        );
      })
    );

    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "existing@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorText = container.querySelector(".text-danger");
      expect(errorText.textContent).toContain("email вже існує");
    });
  });

  it("should display error message when password is less than 6 characters", async () => {
    server.use(
      http.post("*/api/auth/register", () => {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Password: ["Пароль має бути не менше 6 символів"]
            },
            traceId: "00-2fc0b32a057b5c5b0ca76845cf07bb99-725d4d16b82b5b5f-00"
          },
          { status: 400 }
        );
      })
    );

    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "123" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "123" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorText = container.querySelector(".text-danger");
      expect(errorText.textContent).toContain("не менше 6 символів");
    });
  });

  it("should display error message when password lacks a digit", async () => {
    server.use(
      http.post("*/api/auth/register", () => {
        return HttpResponse.json(
          {
            "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            "title": "One or more validation errors occurred.",
            "status": 400,
            "errors": {
                "Password": [
                    "Пароль має містити хоча б одну цифру"
                ]
            },
            "traceId": "00-0b12efe8722cacfb1680c98c179c18d2-9e6a79fbe45c77d4-00"
          },
          { status: 400 }
        );
      })
    );

    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "abcdef" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "abcdef" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorText = container.querySelector(".text-danger");
      expect(errorText.textContent).toContain("хоча б одну цифру");
    });
  });

  //services/api.js
  describe("edge tests", () => {
    it("should catch network errors through Axios interceptor and display global message", async () => {
        server.use(
        http.post("*/api/auth/register", () => {
            return HttpResponse.error(); // ERR_NETWORK
        })
        );

        fireEvent.change(usernameInput, { target: { value: "ananas" } });
        fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
        fireEvent.change(passwordInput, { target: { value: "Password123!" } });
        fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
        fireEvent.click(submitButton);

        await waitFor(() => {
        expect(container.textContent).toContain("Здається, у вас зник інтернет");
        });
    });

    it("should display server down message when status code is 500", async () => {
        server.use(
        http.post("*/api/auth/register", () => {
            return new HttpResponse(null, { status: 503 });
        })
        );

        fireEvent.change(usernameInput, { target: { value: "ananas" } });
        fireEvent.change(emailInput, { target: { value: "ananas@gmail.com" } });
        fireEvent.change(passwordInput, { target: { value: "Password123!" } });
        fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
        fireEvent.click(submitButton);

        await waitFor(() => {
        expect(container.textContent).toContain("Не вдалося з'єднатися з сервером. Спробуйте пізніше.");
        });
    });
  });
});