/**
 * A utility for dynamically updating form state based on an input's `name` attribute.
 */
export const createInputChangeHandler = (setFormData) => (e) => {
  const { name, value } = e.target;
  setFormData((prev) => ({
    ...prev,
    [name]: value,
  }));
};