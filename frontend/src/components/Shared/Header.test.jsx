import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import Header from "./Header";
import { expect } from "vitest";

describe("Base Interaction and API (Logout/Routing)", () => {
  let container;
  let headerElement;
  let burgerButton;

  beforeEach(() => {
    const mockUser = { roles: ["User"], username: "Anastasiia" };
    const rendered = render(
      <BrowserRouter>
        <Header currentUser={mockUser} />
      </BrowserRouter>,
    );
    container = rendered.container;
    headerElement = container.querySelector(".header");
    burgerButton = container.querySelector(".header__hamburger-menu");
  });

  it("should successfully render application title", () => {
    const title = container.querySelector(".header__title");
    expect(title).not.toBeNull();
    expect(title.textContent).toBe("Flashcards Maker");
  });

  it("should toggle active class on hamburger menu when clicked", () => {
    expect(burgerButton).not.toHaveClass("active");

    fireEvent.click(burgerButton);
    expect(burgerButton).toHaveClass("active");

    fireEvent.click(burgerButton);
    expect(burgerButton).not.toHaveClass("active");
  });

  it("should automatically close the hamburger menu when a navigation link is clicked", async () => {
    expect(headerElement).not.toHaveClass("header--open");

    fireEvent.click(burgerButton);
    expect(headerElement).toHaveClass("header--open");

    const link = container.querySelector('.header__link[href="/my-sets"]');
    expect(link).not.toBeNull();
    fireEvent.click(link);

    await waitFor(() => {
      expect(headerElement).not.toHaveClass("header--open");
    });
  });

  it("should successfully call logout API and handle window reload on submit", async () => {
    let apiCalled = false;

    server.use(
      http.post("*/api/auth/logout", () => {
        apiCalled = true;
        return HttpResponse.json({ message: "Вихід успішний" });
      }),
    );

    const fakeLocation = { href: "http://localhost:5173" };
    vi.stubGlobal("location", fakeLocation);

    const logoutForm = container.querySelector(".header__logout-form");

    // Trigger submit and wait for the async execution
    fireEvent.submit(logoutForm);

    await waitFor(() => {
      expect(apiCalled).toBe(true);
      expect(fakeLocation.href).toBe("/");
    });

    // Clean up global mock after the test
    vi.unstubAllGlobals();
  });
});

describe("User Role Navigation and Profile", () => {
  it("should render links and a custom avatar specifically for the User role", () => {
    const mockUser = {
      roles: ["User"],
      username: "Anastasiia",
      avatarUrl: "https://example.com/avatar.jpg",
    };

    const { container } = render(
      <BrowserRouter>
        <Header currentUser={mockUser} />
      </BrowserRouter>,
    );

    const links = container.querySelectorAll(".header__link");
    const linksText = Array.from(links).map((link) => link.textContent.trim());

    expect(linksText).toContain("Головна");
    expect(linksText).toContain("Мої сети");
    expect(linksText).toContain("Мої колекції");
    expect(linksText).toContain("Профіль");
    expect(linksText).toContain("Налаштування");

    // Check if the admin links are not here
    expect(linksText).not.toContain("Скарги");

    // Check avatar
    const avatarImg = container.querySelector(".header__nav-avatar-mini img");
    expect(avatarImg).not.toBeNull();
    expect(avatarImg.getAttribute("src")).toBe(mockUser.avatarUrl);
  });

  it("should fall back to default profile icon if avatarUrl is missing", () => {
    const userWithoutAvatar = { roles: ["User"], username: "Anastasiia" };

    const { container } = render(
      <BrowserRouter>
        <Header currentUser={userWithoutAvatar} />
      </BrowserRouter>,
    );

    const fallbackAvatar = container.querySelector(
      ".header__nav-avatar-mini img",
    );
    expect(fallbackAvatar).not.toBeNull();
  });

  //Add admin panel testing
});
