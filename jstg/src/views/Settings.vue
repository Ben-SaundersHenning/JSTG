<script lang="ts" setup>

    import { ref, onMounted } from "vue";
    import { useForm } from "vee-validate";
    import { toTypedSchema } from "@vee-validate/zod";
    import { z } from "zod";
    import { invoke } from "@tauri-apps/api/core";

    interface Config {
        user_config: UserConfig,
        advanced: Advanced
    }

    interface UserConfig {
        document_save_path: string
    }

    interface Advanced {
        document_api_url: string
    }

    // form schema
    const { errors, handleSubmit, setFieldError, defineField, setFieldValue } = useForm({
        validationSchema: toTypedSchema(z.object({
            user_config: z.object({
                document_save_path: z.string().trim().min(1),
            }),
            advanced: z.object({
                document_api_url: z.string().trim().min(1),
            }),

        }))
    });

    // fields
    const [documentSavePath, documentSavePathAtrb] = defineField("user_config.document_save_path");
    const [documentApiURL, documentApiURLAtrb] = defineField("advanced.document_api_url");

    const onSubmit = handleSubmit(onSuccess, onInvalidSubmit);

    function onSuccess(values) {

        invoke('verify_directory', { directory: values.user_config.document_save_path }).then((truthy) => {
            if(truthy) {
                invoke('update_config', { config: values });
            }
            else {
                setFieldError('user_config.document_save_path', 'Not a valid path');
            }
        })

    }

    function onInvalidSubmit({ values, errors, results }) {
        console.log(errors);
    }

    onMounted(async () => {
        try {
            const config = await invoke<Config>('get_config');
            setFieldValue('user_config.document_save_path', config.user_config.document_save_path);
            setFieldValue('advanced.document_api_url', config.advanced.document_api_url);
        } catch (e) {
            console.log(e)
        }
    })

</script>

<template>
    <form @submit="onSubmit">

        <div class="inputs">
            <div class="path-input vertical-input">
                <p class="input-label">Document Save Path</p>
                <input style="width: auto" aria-label="Document Save Path" id="savepath-input" class="input-border" type="text" name="savepath" v-model="documentSavePath" :="documentSavePathAtrb"/>
                <span class="error">{{errors['user_config.document_save_path']}}</span>
            </div>
        </div>
        <div class="inputs">
            <div class="path-input vertical-input">
                <p class="input-label">Document API URL</p>
                <input style="width: auto" aria-label="Document API URL" id="document-api-input" class="input-border" type="text" name="documentApiURL" v-model="documentApiURL" :="documentApiURLAtrb"/>
                <span class="error">{{errors['advanced.document_api_url']}}</span>
            </div>
        </div>
        <div class="horizontal-input" style="justify-content: center; margin-top: 30px;">
            <button class="submit" type="submit">Save</button>
        </div>

    </form>
</template>

<style lang="scss" scoped>

    @use '../variables';

    .inputs {
        margin: 30px;
    }

</style>
