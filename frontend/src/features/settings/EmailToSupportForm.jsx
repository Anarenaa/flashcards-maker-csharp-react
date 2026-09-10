import z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import { handleServerErrors } from "../../utils/formHandlers";
import "./EmailToSupportForm.scss";
import { useMutation } from "@tanstack/react-query";
import api from "../../services/api";
import toast from "react-hot-toast";

const letterSchema = z.object({
  subject: z
    .string()
    .trim()
    .min(1, "Введіть тему листа")
    .max(100, "Забагато символів"),
  message: z
    .string()
    .trim()
    .min(1, "Введіть текст-звернення")
    .max(2000, "Забагато символів"),
});

export default function EmailToSupport({ isOpen, onClose }) {
  const {
    register,
    reset,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(letterSchema),
  });

  const sendMutation = useMutation({
    mutationFn: (dataForm) => api.post("/emails/to-support", dataForm),
    onSuccess: () => {
      reset();
      onClose();
      toast.success("Ваш лист успішно надіслано.");
    },
    onError: (err) => {
      handleServerErrors(err, setError);
      toast.error("Лист не вдалося надіслати");
    },
  });

  const onSubmit = async (data) => {
    sendMutation.mutate(data);
  };

  const isLoading = isSubmitting || sendMutation.isPending;

  return (
    <ModalWrapper
      isOpen={isOpen}
      onClose={onClose}
      showCloseButton={true}
      isCentered={true}
      size="sm"
    >
      <div className="top-panel">
        <h2 className="title">Лист до тех. підтримки</h2>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="email-to-support-form">
        <div className="form-field">
          <input
            type="text"
            placeholder="Тема"
            className={errors.subject ? "error-input" : ""}
            {...register("subject")}
          />
          {errors.subject && (
            <span className="text-danger">{errors.subject.message}</span>
          )}
        </div>
        <div className="form-field">
          <textarea
            placeholder="Текст..."
            className={errors.message ? "error-input" : ""}
            {...register("message")}
          />
          {errors.message && (
            <span className="text-danger">{errors.message.message}</span>
          )}
        </div>
        <button type="submit" className="primary-button" disabled={isLoading}>
          {isLoading ? "В процесі відправки..." : "Відправити"}
        </button>
      </form>
    </ModalWrapper>
  );
}
