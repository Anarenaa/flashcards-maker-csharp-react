import { renderHook, act } from "@testing-library/react";
import { useState } from "react";
import { createInputChangeHandler } from "./formHandlers";

it("sets email properly", () => {
  const { result } = renderHook(() => {
    const [formData, setFormData] = useState({ userName: "ananas", email: "" });
    const handleInputChange = createInputChangeHandler(setFormData);
    return { formData, handleInputChange };
  });

  act(() => {
    result.current.handleInputChange({
      target: { name: "email", value: "ananas@gmail.com" }
    });
  });

  expect(result.current.formData).toEqual({
    userName: "ananas",
    email: "ananas@gmail.com"
  });
});