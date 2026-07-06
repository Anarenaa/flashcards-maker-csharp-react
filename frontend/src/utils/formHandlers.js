
export const handleServerErrors = (err, setError) => {
  if (err.globalMessage) {
    setError("root.serverError", { message: err.globalMessage });
  } else if (err.response && err.response.data) {
    const serverErrors = err.response.data.errors || err.response.data;

    Object.keys(serverErrors).forEach((key) => {
      if (key.toLowerCase() === "global") {
        setError("root.serverError", { message: serverErrors[key][0] });
      } else {
        // (UserName -> userName)
        const fieldName = key.charAt(0).toLowerCase() + key.slice(1);
        setError(fieldName, { message: serverErrors[key][0] });
      }
    });
  }
};