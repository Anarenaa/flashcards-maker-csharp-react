import toast from "react-hot-toast";

export const handleServerErrors = (err, setError) => {
  if (err.response && err.response.data) {
    const backendErrors = err.response.data.errors || err.response.data;

    Object.keys(backendErrors).forEach((key) => {
      const value = backendErrors[key];
      // Ignore non-field responses like { message: "..." } (e.g. 503)
      if (!Array.isArray(value) || value.length === 0) return;

      if (key.toLowerCase() === "global") {
        toast.error(value[0], { className: "toast-error" });
      } else {
        const fieldName = key.charAt(0).toLowerCase() + key.slice(1);
        setError(fieldName, { message: value[0] });
      }
    });
  }
};