import toast from "react-hot-toast";

export const handleServerErrors = (err, setError) => {
    if (err.response && err.response.data) {
    const backendErrors = err.response.data.errors || err.response.data;

    Object.keys(backendErrors).forEach((key) => {
      if (key.toLowerCase() === "global") {
        toast.error(backendErrors[key][0], { className: "toast-error" });
      } else {
        // (UserName -> userName)
        const fieldName = key.charAt(0).toLowerCase() + key.slice(1);
        setError(fieldName, { message: backendErrors[key][0] });
      }
    });
  }
};