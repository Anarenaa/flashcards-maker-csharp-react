import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import z from "zod";
import { useParams } from "react-router";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import { handleServerErrors } from "../../utils/formHandlers";
import api from "../../services/api";
import "./FlashcardContextForm.scss";

const contextSchema = z.object({
  sentence: z
    .string()
    .trim()
    .min(1, "Введіть приклад речення")
    .max(100, "Занадто багато символів"),
  translation: z
    .string()
    .trim()
    .min(1, "Переклад речення обов'язковий")
    .max(100, "Занадто багато символів"),
});

export default function FlashcardContextForm({
  isOpen,
  onClose,
  card,
  initialData,
}) {
  const { id: setId } = useParams();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(contextSchema),
    // Automatically fills and updates input when editing
    values: initialData
      ? { sentence: initialData.sentence, translation: initialData.translation }
      : { sentence: "", translation: "" },
  });

  const createMutation = useMutation({
    mutationFn: (formData) =>
      api
        .post(`/sets/${setId}/flashcards/${card.id}/contexts`, formData)
        .then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries(["flashcardContexts", setId, card.id]);
      reset();
      onClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, formData }) =>
      api
        .put(`/sets/${setId}/flashcards/${card.id}/contexts/${id}`, formData)
        .then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries(["flashcardContexts", setId, card.id]);
      reset();
      onClose();
    },
  });

  const onSubmit = async (data) => {
    try {
      if (initialData) {
        await updateMutation.mutateAsync({
          id: initialData.id,
          formData: data,
        });
      } else {
        await createMutation.mutateAsync(data);
      }
    } catch (error) {
      handleServerErrors(error, setError);
    }
  };

  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} showCloseButton={true} size="sm">
      <div className="top-panel">
        <h2 className="title">
          {initialData ? "Редагувати" : "Додати контекст"}
        </h2>
      </div>
      <form
        className="flashcard-context-form"
        onSubmit={handleSubmit(onSubmit)}
      >
        <div className="form-field">
          <label htmlFor="context-sentence">Речення</label>
          <input
            id="context-sentence"
            {...register("sentence")}
            type="text"
            placeholder={`Введіть речення зі словом "${card.term}"`}
            className={errors.sentence ? "error-input" : ""}
          />
          {errors.sentence && (
            <span className="text-danger">{errors.sentence.message}</span>
          )}
        </div>

        <div className="form-field">
          <label htmlFor="context-translation">Переклад</label>
          <input
            id="context-translation"
            {...register("translation")}
            type="text"
            placeholder="Введіть переклад"
            className={errors.translation ? "error-input" : ""}
          />
          {errors.translation && (
            <span className="text-danger">{errors.translation.message}</span>
          )}
        </div>

        <button
          type="submit"
          className="primary-button"
          disabled={isSubmitting || createMutation.isPending}
        >
          {isSubmitting || createMutation.isPending
            ? "Збереження..."
            : "Зберегти"}
        </button>
      </form>
    </ModalWrapper>
  );
}
