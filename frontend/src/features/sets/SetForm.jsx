import { useState, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";
import z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import { useSetTypes } from "../../hooks/useSetTypes";
import { handleServerErrors } from "../../utils/formHandlers";
import { LANGUAGES } from "../../constants/languages";
import { ArrowRight } from "lucide-react";
import api from "../../services/api";
import "./SetForm.scss";

const setSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Введіть назву сету")
    .min(2, "Мінімальна кількість символів - 2")
    .max(100, "Занадто багато символів"),
  description: z.string().max(500, "Занадто багато символів"),
  type: z.coerce.number(),
  isPublic: z.boolean(),
  toLang: z.string().min(1, "Виберіть мову"),
  fromLang: z.string().optional(),
});

export default function SetForm({ isOpen, onClose, initialData }) {
  const [isAiEnabled, setIsAiEnabled] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const types = useSetTypes().slice().reverse();

  const defaultToLang = "uk";
  const defaultFromLang = defaultToLang === "uk" ? "en" : "uk";

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(setSchema),
    values: initialData
      ? {
          type: initialData.type,
          name: initialData.name,
          description: initialData.description,
          isPublic: initialData.isPublic,
          toLang: initialData.toLang,
          fromLang: initialData.fromLang,
        }
      : {
          type: types[0]?.id ?? 0,
          name: "",
          description: "",
          isPublic: false,
          toLang: defaultToLang,
          fromLang: defaultFromLang,
        },
  });

  const currentSetType = watch("type");

  const createMutation = useMutation({
    mutationFn: (formData) =>
      api.post(`/my-sets`, formData).then((res) => res.data),
    onSuccess: (newSet) => {
      queryClient.invalidateQueries(["sets"]);
      reset();
      onClose();
      // setTimeout(() => {
      //   navigate(`/my-sets/${newSet.id}`);
      // }, 200);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, formData }) => api.put(`/my-sets/${id}`, formData),
    onSuccess: () => {
      queryClient.invalidateQueries(["sets"]);
      reset();
      onClose();
    },
  });

  const onSubmit = async (data) => {
    try {
      const payload = {
        ...data,
        // fromLang = toLang in subject sets
        fromLang: Number(data.type) === 0 ? data.fromLang : data.toLang,
      };

      if (initialData) {
        await updateMutation.mutateAsync({
          id: initialData.id,
          formData: payload,
        });
      } else {
        await createMutation.mutateAsync(payload);
      }
    } catch (error) {
      handleServerErrors(error, setError);
    }
  };

  const isPending =
    isSubmitting || createMutation.isPending || updateMutation.isPending;

  return (
    <ModalWrapper
      isOpen={isOpen}
      onClose={onClose}
      showCloseButton={true}
      size="sm"
    >
      <div className="set-form">
        <div className="top-panel">
          <h2 className="title">
            {initialData ? "Редагування сету" : "Новий сет"}
          </h2>
          <label
            className="toggle-switch ai-switch"
            htmlFor="ai-checkbox-input"
          >
            <input
              type="checkbox"
              id="ai-checkbox-input"
              checked={isAiEnabled}
              onChange={(e) => setIsAiEnabled(e.target.checked)}
            />
            <span className="toggle-slider"></span>
            <span className="toggle-label ai-toggle-label">ШІ</span>
          </label>
        </div>
        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="form-field">
            <label htmlFor="set-type-select">
              Що вивчаємо <span className="require">*</span>
            </label>
            <select
              id="set-type-select"
              className="sort-select"
              {...register("type", { valueAsNumber: true })}
            >
              {types.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </div>

          <div className="languages">
            {Number(currentSetType) === 0 && (
              <>
                <div className="form-field">
                  <label htmlFor="from-lang-select">
                    Мова термінів <span className="require">*</span>
                  </label>
                  <select
                    id="from-lang-select"
                    className="sort-select"
                    {...register("fromLang")}
                  >
                    {LANGUAGES.map((lang) => (
                      <option key={lang.code} value={lang.code}>
                        {lang.name}
                      </option>
                    ))}
                  </select>
                </div>
                <ArrowRight size={20} className="arrow-icon" />
              </>
            )}
            <div className="form-field">
              <label htmlFor="to-lang-select">
                {Number(currentSetType) === 0 ? "Мова означень" : "Мова сету "}
                <span className="require">*</span>
              </label>
              <select
                id="to-lang-select"
                className="sort-select"
                {...register("toLang")}
              >
                {LANGUAGES.map((lang) => (
                  <option key={lang.code} value={lang.code}>
                    {lang.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-field">
            <label htmlFor="set-name-input">
              Назва сету <span className="require">*</span>
            </label>
            <input
              type="text"
              id="set-name-input"
              {...register("name")}
              className={errors.name ? "error-input" : ""}
            />
            {errors.name && (
              <span className="text-danger">{errors.name.message}</span>
            )}
          </div>
          <div className="form-field">
            <label htmlFor="set-description-input">Опис сету</label>
            <textarea
              id="set-description-input"
              {...register("description")}
              className={errors.description ? "error-input" : ""}
            ></textarea>
            {errors.description && (
              <span className="text-danger">{errors.description.message}</span>
            )}
          </div>
          <label
            className="toggle-switch is-public-switch"
            htmlFor="is-public-checkbox-input"
          >
            <span className="toggle-label">Публічний</span>
            <input
              type="checkbox"
              id="is-public-checkbox-input"
              {...register("isPublic")}
            />
            <span className="toggle-slider"></span>
          </label>

          <button type="submit" className="primary-button" disabled={isPending}>
            {isPending ? "Збереження..." : "Зберегти"}
          </button>
        </form>
      </div>
    </ModalWrapper>
  );
}
