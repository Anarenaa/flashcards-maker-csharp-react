export function formatLocalDate(dateString, { withTime = true } = {}) {
  if (!dateString) return "";
  const isoString = dateString.endsWith("Z") ? dateString : dateString + "Z";
  
  const options = {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    ...(withTime && {
      hour: "2-digit",
      minute: "2-digit",
    }),
  };

  return new Date(isoString).toLocaleDateString(undefined, options);
}