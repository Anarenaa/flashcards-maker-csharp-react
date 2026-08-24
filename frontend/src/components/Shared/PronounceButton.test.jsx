import { render, fireEvent, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import PronounceButton from "./PronounceButton";

// Mock the browser voices loading function
vi.mock("../../utils/loadVoices", () => ({
  loadVoices: vi.fn().mockResolvedValue([{ lang: "en-US" }]),
}));

// Mock global SpeechSynthesis to prevent tests from crashing in the Node.js environment
beforeAll(() => {
  global.SpeechSynthesisUtterance = vi.fn();
  global.window.speechSynthesis = {
    cancel: vi.fn(),
    speak: vi.fn(),
  };
  // Mock the Audio constructor so it doesn't attempt to actually play real sound
  global.Audio = vi.fn().mockImplementation(() => ({
    play: vi.fn().mockResolvedValue(),
  }));
  // Mock createObjectURL
  global.URL.createObjectURL = vi.fn().mockReturnValue("blob:test-audio-url");
});

it("should render pronunciation button with Volume icon", () => {
  const { container } = render(<PronounceButton word="hello" lang="en" />);
  expect(container.querySelector(".speak-btn")).not.toBeNull();
});

it("should use browser speech synthesis when language is supported", async () => {
  const { container } = render(<PronounceButton word="hello" lang="en" />);

  await new Promise((resolve) => setTimeout(resolve, 50));

  fireEvent.click(container.querySelector(".speak-btn"));

  expect(window.speechSynthesis.speak).toHaveBeenCalled();
});

it("should fallback to backend TTS when language is not supported", async () => {
  let apiCalled = false;
  server.use(
    http.post("*/api/tts", () => {
      apiCalled = true;
      return new HttpResponse(new ArrayBuffer(8), {
        headers: { "Content-Type": "audio/mpeg" },
      });
    }),
  );

  const { container } = render(<PronounceButton word="привіт" lang="uk" />);

  await waitFor(() => {
    fireEvent.click(container.querySelector(".speak-btn"));
  });

  await waitFor(() => {
    expect(apiCalled).toBe(true);
  });
});
